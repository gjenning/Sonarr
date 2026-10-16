# Task 2.3: AudioFormatController (CRUD + /schema)

Status: DONE
Phase: 2
Depends: 1.4, 1.5, 2.2
Verify: `dotnet build src/Sonarr.sln -c Debug`

## Goal
REST API `/api/v3/audioformat` with CRUD + `GET /schema` (spec field metadata for the UI),
mirroring the Custom Formats controller.

## Context
Template: `src/Sonarr.Api.V3/CustomFormats/CustomFormatController.cs` (+ `CustomFormatResource.cs`,
`CustomFormatSpecificationSchema.cs` in the same folder). Key points:
- Route `api/v3/audioformat`; resource = `id, name, specifications[]` where each spec has
  `name, implementation` (→ `AudioStreamSpecification`), `order, negate, required, value` plus
  the schema's `valueOverride`/options metadata.
- `GET schema`: reflect `IAudioFormatSpecification` implementations (the CF controller builds
  it from the spec registry + `FieldDefinition`s) — the `Field` enum on `AudioStreamSpecification`
  must surface its options so the UI can render a field selector. If CF's schema is per-spec-class
  with one schema per class, expose a single `AudioStreamSpecification` schema whose `field`
  property carries the enum options (simplest; document it).
- Validation on save: duplicate name (service throws `ValidationException` → API maps it),
  invalid regex → 400.
- Check how controllers get registered (NancyFx `NancyModule` subclass auto-discovery in this
  codebase — confirm by reading CustomFormatController's base class/registration).

## Files
- Create: `src/Sonarr.Api.V3/AudioFormats/AudioFormatController.cs`
- Create: `src/Sonarr.Api.V3/AudioFormats/AudioFormatResource.cs`
- Create: `src/Sonarr.Api.V3/AudioFormats/AudioFormatSpecificationSchema.cs` (only if CF has a
  separate schema file — mirror structure)

## Acceptance Criteria
- [x] GET/POST/PUT/DELETE `/api/v3/audioformat` work; `GET /schema` returns field options
      including `Language` (so the UI can show the language selector) and `AudioDescription`.
- [x] Solution builds.

## Result
Created the Audio Formats API mirroring the Custom Formats controller:

- `src/Sonarr.Api.V3/AudioFormats/AudioFormatController.cs` — `[V3ApiController]` (route
  `api/v3/audioformat`, derived from `[controller]` + LowercaseUrls). Endpoints: `GET /` (all,
  with details), `GET /{id}`, `POST`, `PUT`, `DELETE /{id}`, `GET /schema`. `SharedValidator`
  enforces non-empty unique `Name` and non-empty `Specifications`; per-spec `Validate()`
  (invalid regex / bad numeric comparison / unknown language id) throws `ValidationException`
  → 400 via the REST pipeline.
- `src/Sonarr.Api.V3/AudioFormats/AudioFormatResource.cs` — `id, name, specifications[]` +
  mapper; `ToModel` resolves each spec's `implementation` against the injected
  `List<IAudioFormatSpecification>` (DryIoc auto-registers implementations) and rebuilds the
  spec via `SchemaBuilder.ReadFromSchema`.
- `src/Sonarr.Api.V3/AudioFormats/AudioFormatSpecificationSchema.cs` — schema resource +
  `ToSchema` extension (same shape as CF's).

Deviations:
- Schema is a single `AudioStreamSpecification` entry whose `field` property carries the
  `AudioStreamField` enum select options (Codec, Channels, Bitrate, Title, Language,
  AudioDescription, StreamCount) instead of one entry per spec class — as anticipated in
  Context; documented in a code comment in `GetTemplates`.
- No bulk endpoints (`bulk` PUT/DELETE) mirrored: CF's bulk-update only toggles
  `IncludeCustomFormatWhenRenaming`, which has no audio equivalent, and task scope is CRUD + schema.
- Presets in `/schema` are clones of existing audio formats' specs only (no hardcoded presets,
  since audio specs target stream metadata, not release titles) — commented in code.

Tests: added `src/NzbDrone.Api.Test/v3/AudioFormats/AudioFormatResourceFixture.cs` (3 tests:
resource round-trip preserves specs, list view omits specs, schema exposes field select
options) — substitute for manual API testing in this environment. `dotnet build src/Sonarr.sln
-c Debug` succeeds (0 warnings); Api.Test fixture 3/3 pass; Core AudioFormat tests 94/94 pass.
