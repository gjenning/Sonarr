# Task 1.2: IAudioFormatSpecification + AudioFormatInput

Status: DONE
Phase: 1
Depends: -
Verify: `dotnet build src/NzbDrone.Core/Sonarr.Core.csproj -c Debug`

## Goal
Create the specification plumbing for audio formats: the spec interface, the input object handed
to specs at evaluation time, and the JSON polymorphism wiring for embedded spec lists.

## Context
Mirror `src/NzbDrone.Core/CustomFormats/`:
- `ICustomFormatSpecification` → `IAudioFormatSpecification` (IsSatisfiedBy(input), IsSatisfiedBy
  schema/validation members as the CF interface has).
- `CustomFormatInput` (Episode, ReleaseInfo, FileInfo, Language) → `AudioFormatInput` with just
  what audio matching needs: `MediaInfo MediaInfo` (the `MediaInfoModel`) and `string SceneName`
  (release title — `MediaInfoFormatter.FormatAudioCodec` uses it for heuristic codec naming).
- CF specs are stored as an embedded JSON list on the entity; the polymorphic deserialization is
  handled by a custom serializer registered in `src/NzbDrone.Core/Datastore/TableMapping.cs`
  (search `CustomFormat` there). Task 1.3 registers the audio equivalent; this task only creates
  the interface + input so 1.3/1.5 can compile.

## Files
- Create: `src/NzbDrone.Core/AudioFormats/IAudioFormatSpecification.cs`
- Create: `src/NzbDrone.Core/AudioFormats/AudioFormatInput.cs`

## Steps
1. Copy the shape of `CustomFormats/ICustomFormatSpecification.cs` → `IAudioFormatSpecification`
  (namespace `NzbDrone.Core.AudioFormats`), typed to `AudioFormatInput`. Keep `Name`, `Order`,
  `Negate`, `Required` members exactly as CF does (schema endpoint in 2.3 depends on it).
2. Create `AudioFormatInput` record/class with `MediaInfo` and `SceneName`.
3. Build Core project.

## Acceptance Criteria
- [ ] Interface + input compile; no references to CustomFormats internals left dangling.
- [ ] Namespace `NzbDrone.Core.AudioFormats`.

## Result
- Created `src/NzbDrone.Core/AudioFormats/IAudioFormatSpecification.cs` — mirrors
  `ICustomFormatSpecification` (Order, InfoLink, ImplementationName, Name, Negate, Required,
  Validate, Clone, IsSatisfiedBy) typed to `AudioFormatInput`.
- Created `src/NzbDrone.Core/AudioFormats/AudioFormatInput.cs` — `MediaInfo` (MediaInfoModel) +
  `SceneName`.
- Build succeeded, 0 warnings. No references to CustomFormats internals.
