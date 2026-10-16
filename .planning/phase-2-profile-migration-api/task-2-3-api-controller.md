# Task 2.3: AudioFormatController (CRUD + /schema)

Status: TODO
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
- [ ] GET/POST/PUT/DELETE `/api/v3/audioformat` work; `GET /schema` returns field options
      including `Language` (so the UI can show the language selector) and `AudioDescription`.
- [ ] Solution builds.

## Result
_(fill on completion)_
