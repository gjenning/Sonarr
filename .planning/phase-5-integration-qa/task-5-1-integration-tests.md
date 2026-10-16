# Task 5.1: API integration test (NzbDrone.Integration.Test)

Status: TODO
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
- [ ] Test passes locally (`dotnet test src/NzbDrone.Integration.Test --filter ~AudioFormat`).
- [ ] Covers happy path + validation failure.

## Result
_(fill on completion)_
