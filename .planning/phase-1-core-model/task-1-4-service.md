# Task 1.4: AudioFormatService (CRUD)

Status: DONE
Phase: 1
Depends: 1.3
Verify: `dotnet build src/NzbDrone.Core/Sonarr.Core.csproj -c Debug -p:SolutionDir=$PWD/src/`

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
- [x] CRUD works against repository; duplicate name rejected.
- [x] Core builds.

## Result
- Created `AudioFormatService.cs` with `IAudioFormatService` + `AudioFormatService` in one file,
  mirroring `CustomFormatService`: cached `All()`/`GetById()` (Dictionary cache keyed by Id,
  cleared on every mutation), `Insert`/`Update`(single + list)/`Delete`(single + list).
- CF does fire events, so mirrored them: created `AudioFormats/Events/AudioFormatAddedEvent.cs`
  and `AudioFormatDeletedEvent.cs` (published on Insert / before Delete, same as CF). No
  subscribers yet — profile cleanup wiring arrives with phase 2.
- DI: confirmed convention-based (grep found no manual registration of `ICustomFormatService`),
  so no registration needed for `IAudioFormatService`.
- Deviation: CF's service has NO duplicate-name validation (CF enforces uniqueness in the API
  controller with "Must be unique."). Per this task's spec, uniqueness lives in the service:
  case-insensitive name check excluding own Id, throws `ValidationException` with
  `NzbDroneValidationFailure("Name", "name already exists")`. Task 2.3 (controller) can add its
  own validator on top if desired.
- Deviation: the listed Verify command fails on a clean tree — building a bare csproj leaves
  `$(SolutionDir)` undefined, so `src/stylecop.json` isn't passed to StyleCop and SA1200 fires
  across the whole codebase (TreatWarningsAsErrors promotes it). Fixed by appending
  `-p:SolutionDir=$PWD/src/`. Build: succeeded, 0 warnings, 0 errors.
- No unit tests added (none specified for this task; scoring/service fixtures come with later
  tasks). CRUD + duplicate-rejection verified by build + code review against the CF original.
