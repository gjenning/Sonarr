# Task 5.1: API integration test (NzbDrone.Integration.Test)

Status: DONE
Phase: 5
Depends: 2.3, 2.4
Verify: `dotnet test src/NzbDrone.Integration.Test -c Debug --filter "FullyQualifiedName~AudioFormat"`

## Goal
Integration coverage for the new endpoints: CRUD round-trip on `/api/v3/audioformat` and the new
profile fields round-tripping through `/api/v3/qualityprofile`.

## Context
Pattern: `src/NzbDrone.Integration.Test/ApiTests/` — find the Custom Formats test
(`CustomFormatFixture.cs` or similar; grep `customformat` in that folder) and mirror:
- POST an audio format with one `AudioStreamSpecification` (Codec regex) → GET list contains it →
  PUT rename → GET by id reflects change → DELETE → gone.
- GET `/api/v3/audioformat/schema` returns non-empty with the `field` enum options.
- GET a quality profile, set `audioFormatItems`/`minAudioFormatScore`/`upgradeForAudio` via PUT,
  re-GET and assert.
- Duplicate-name POST → 4xx with validation message.

## Files
- Create: `src/NzbDrone.Integration.Test/ApiTests/AudioFormatFixture.cs`

## Acceptance Criteria
- [x] Test passes locally (`dotnet test src/NzbDrone.Integration.Test --filter ~AudioFormat`).
- [x] Covers happy path + validation failure.

## Result
Created `src/NzbDrone.Integration.Test/ApiTests/AudioFormatFixture.cs` (only file touched) with 4
tests, all passing:

- `schema_should_contain_audio_stream_field_options` — GET `/api/v3/audioformat/schema` non-empty,
  single `AudioStreamSpecification` entry, `field` select options contain all 7 `AudioStreamField`
  enum names.
- `should_add_and_delete_audio_format` — POST (Codec regex spec built by round-tripping the server
  schema, like the frontend does) → GET list contains it → spec/field values round-trip → PUT rename
  → GET by id reflects change → DELETE → gone from list.
- `should_not_allow_duplicate_names` — duplicate-name POST → 400 with "Must be unique." validation
  message (asserted on the JSON error array body).
- `should_round_trip_audio_format_profile_fields` — POST audio format, then PUT a quality profile
  with `audioFormatItems` (score 100), `minAudioFormatScore` (50), `cutoffAudioFormatScore` (100,
  required ≥ min by the profile validator) and `upgradeForAudio`; re-GET and assert.

Deviations / notes:
- No CustomFormat integration fixture exists in this repo to mirror; used `IndexerFixture`
  (schema round-trip) + `RootFolderFixture` (InvalidPost) patterns instead, with a local
  `ClientBase<AudioFormatResource>` created inside the fixture (no change to `IntegrationTestBase`).
- Dropped the planned "GET by id after DELETE → 404" assertion: `AudioFormatService.GetById` throws
  `KeyNotFoundException` (→ 500) for missing ids — identical to the existing `CustomFormatService`
  behavior, so this matches the codebase pattern rather than being a new defect. Absence after
  DELETE is asserted via the list instead. If 404-on-missing is ever wanted, fix both services.
- Verify command as written fails to build: `stylecop.json` is included via `$(SolutionDir)`, which
  is undefined when building/testing a single project (SA1200 floods). Workaround used:
  `dotnet build src/Sonarr.sln -c Debug` first, then
  `dotnet test src/NzbDrone.Integration.Test -c Debug --filter "FullyQualifiedName~AudioFormat" -p:SolutionDir=$PWD/src/`.
