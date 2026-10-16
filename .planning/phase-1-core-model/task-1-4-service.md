# Task 1.4: AudioFormatService (CRUD)

Status: TODO
Phase: 1
Depends: 1.3
Verify: `dotnet build src/NzbDrone.Core/Sonarr.Core.csproj -c Debug`

## Goal
Service layer for managing audio formats: Add/Update/List/FetchAll/Delete with name-uniqueness
validation, mirroring `CustomFormatService`.

## Context
Mirror `src/NzbDrone.Core/CustomFormats/CustomFormatService.cs` (`ICustomFormatService` +
`CustomFormatService`). CF fires events on change (e.g. `CustomFormatsUpdatedEvent`?) — check the
CF service; if it does, mirror with audio equivalents, if not, keep it simple. DI registration in
Sonarr is convention-based (Ninject auto-registers `IXxx`→`Xxx`), so no manual registration
should be needed — verify by grepping how `ICustomFormatService` is registered.

## Files
- Create: `src/NzbDrone.Core/AudioFormats/AudioFormatService.cs` (interface + impl in one file,
  matching CF's file layout)

## Steps
1. Copy `CustomFormatService.cs` structure → `IAudioFormatService` / `AudioFormatService`.
2. Keep duplicate-name validation (case-insensitive) throwing `ValidationException` with the
   same message style ("name already exists").
3. Build.

## Acceptance Criteria
- [ ] CRUD works against repository; duplicate name rejected.
- [ ] Core builds.

## Result
_(fill on completion)_
