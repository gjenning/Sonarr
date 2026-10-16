# Task 1.3: AudioFormat entity + repository + TableMapping

Status: TODO
Phase: 1
Depends: 1.2
Verify: `dotnet build src/NzbDrone.Core/Sonarr.Core.csproj -c Debug`

## Goal
`AudioFormat` persisted entity with an embedded specification list, mirroring `CustomFormat`.

## Context
Mirror `src/NzbDrone.Core/CustomFormats/CustomFormat.cs`:
- `class AudioFormat : ModelBase` with `Name`, `Specifications` (`List<IAudioFormatSpecification>`),
  `IncludeCustomFormatWhenRenaming` equivalent is NOT needed — skip it.
- Repository: `AudioFormatRepository : BasicRepository<AudioFormat>` in
  `src/NzbDrone.Core/AudioFormats/AudioFormatRepository.cs` (mirror
  `CustomFormats/CustomFormatRepository.cs`).
- Table registration in `src/NzbDrone.Core/Datastore/TableMapping.cs`: find how `CustomFormat`
  registers its `Specifications` list (custom JSON serializer for the polymorphic interface
  list) and register `AudioFormat` the same way, table name `AudioFormats`. The actual DB
  migration is task 2.2 — TableMapping is what DDL-less tests + the ORM use.

## Files
- Create: `src/NzbDrone.Core/AudioFormats/AudioFormat.cs`
- Create: `src/NzbDrone.Core/AudioFormats/AudioFormatRepository.cs`
- Modify: `src/NzbDrone.Core/Datastore/TableMapping.cs`

## Steps
1. Create entity + repository mirroring the CF files (namespace `NzbDrone.Core.AudioFormats`).
2. In TableMapping.cs, add the `AudioFormats` table mapping with the same embedded-list serializer
   treatment as `CustomFormats.Specifications` (reuse the generic serializer helper if one
   exists; otherwise mirror the CF one for `IAudioFormatSpecification`).
3. Build.

## Acceptance Criteria
- [ ] `AudioFormat` maps to table `AudioFormats`; Specifications round-trips through the ORM
      serializer (add a quick round-trip check if a TableMapping test fixture exists —
      `src/NzbDrone.Core.Test/Datastore/` has `CustomFormatTableMappingFixture.cs` to copy).
- [ ] Core builds.

## Result
_(fill on completion)_
