# syntax=docker/dockerfile:1

# --- Build the web UI ---------------------------------------------------------
FROM node:24 AS ui-build
WORKDIR /build

COPY package.json yarn.lock ./
RUN corepack enable && yarn install --frozen-lockfile

COPY vite.config.ts tsconfig.json index.html ./
COPY frontend ./frontend
RUN yarn build

# --- Build and publish the server (framework-dependent) -----------------------
FROM mcr.microsoft.com/dotnet/sdk:10.0.402-noble AS server-build
WORKDIR /build

COPY global.json .editorconfig ./
COPY Logo ./Logo
COPY src ./src
COPY --from=ui-build /build/_output/UI ./_output/UI

# Directory.Build.props defaults the RID to the build container's architecture
# (linux-arm64 / linux-x64) and publish stays framework-dependent.
# SolutionDir must be passed explicitly: it is normally set by solution/IDE
# builds and stylecop.json is resolved through it.
RUN dotnet publish src/NzbDrone.Console/Sonarr.Console.csproj -c Release -f net10.0 \
        -p:SolutionDir=/build/src/ \
        -o /publish \
    && dotnet publish src/NzbDrone.Mono/Sonarr.Mono.csproj -c Release -f net10.0 \
        -p:SolutionDir=/build/src/ \
        -o /publish \
    && cp -r /build/_output/UI /publish/UI

# --- Runtime image --------------------------------------------------------------
FROM mcr.microsoft.com/dotnet/aspnet:10.0-noble

RUN apt-get update && apt-get install -y --no-install-recommends \
        ffmpeg \
        sqlite3 \
        tzdata \
    && rm -rf /var/lib/apt/lists/*

WORKDIR /app
COPY --from=server-build /publish ./

# Signals to Sonarr that it runs in Docker (disables the built-in updater)
RUN touch /.dockerenv

EXPOSE 8989
VOLUME ["/config", "/data"]

ENTRYPOINT ["dotnet", "/app/Sonarr.dll", "--nobrowser", "--data=/config"]
