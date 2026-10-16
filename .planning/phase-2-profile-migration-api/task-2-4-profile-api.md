# Task 2.4: QualityProfileResource + validator updates (V3 + V5)

Status: TODO
Phase: 2
Depends: 2.1, 2.3
Verify: `dotnet build src/Sonarr.sln -c Debug`

## Goal
Expose the new profile fields over the API (both API versions) and validate them.

## Context
- `QualityProfileResource` lives in `src/Sonarr.Api.V3/Profiles/Quality/QualityProfileResource.cs`;
  the profile **controller** is in the V5 API (`src/Sonarr.Api.V5/Profiles/Quality/`) — grep
  `QualityProfileResource` to find every touch point (mapping extensions `ToResource`/`ToModel`
  in both API layers).
- Add to resource: `audioFormatItems` (id + score pairs, mirror `formatItems` shape),
  `minAudioFormatScore`, `cutoffAudioFormatScore`, `upgradeForAudio`,
  `audioUpgradeRequiresTitleHint`.
- Validators: `src/Sonarr.Api.V3/Profiles/Quality/QualityProfileValidator.cs` (grep for
  `QualityCutoffValidator` / format-score validators) — mirror any cutoff-vs-min sanity rule for
  the audio scores (e.g. cutoff ≥ min when both set). Keep it minimal; don't invent new rules.
- Ensure V3 and V5 resources stay in sync (this repo carries both; check whether V5 reuses V3
  types or duplicates — follow what exists).

## Files
- Modify: `src/Sonarr.Api.V3/Profiles/Quality/QualityProfileResource.cs` (+ controller mapping)
- Modify: V5 profile resource/controller (locate via grep)
- Modify: profile validator(s) as above

## Acceptance Criteria
- [ ] `GET /api/v3/qualityprofile` includes the new fields; PUT round-trips them.
- [ ] Invalid combos (negative scores) rejected if a numeric validator pattern exists.
- [ ] Solution builds.

## Result
_(fill on completion)_
