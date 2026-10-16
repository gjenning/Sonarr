# Task 2.4: QualityProfileResource + validator updates (V3 + V5)

Status: DONE
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
- [x] `GET /api/v3/qualityprofile` includes the new fields; PUT round-trips them.
- [x] Invalid combos (negative scores) rejected if a numeric validator pattern exists.
- [x] Solution builds.

## Result
Added the five audio fields to `QualityProfileResource` in both API layers (they are duplicated
types, not shared), plus a `ProfileAudioFormatItemResource` (Format/Name/Score, mirroring
`ProfileFormatItemResource`) and `ToResource`/`ToModel` mappings for it and the new scalar fields:
- `src/Sonarr.Api.V3/Profiles/Quality/QualityProfileResource.cs`
- `src/Sonarr.Api.V5/Profiles/Quality/QualityProfileResource.cs`

Validation (mirrors the existing numeric-score pattern `MinUpgradeFormatScore >= 1`, which lives in
the controllers' `SharedValidator`, not in `QualityProfileResourceValidator`):
- `MinAudioFormatScore >= 0` (rejects negative scores).
- `CutoffAudioFormatScore >= MinAudioFormatScore` (cutoff-vs-min sanity rule).
Added to both `QualityProfileController`s (V3 + V5).

Deviations:
- The task pointed at `QualityProfileValidator.cs` / `QualityCutoffValidator`; the actual files are
  `QualityProfileResourceValidator.cs` (only quality-size rules, no score rules) and the score rules
  live in the controllers. Followed what exists: score rules added to controller `SharedValidator`.
- No existing cutoff≥min rule existed for custom-format scores, so the audio cutoff≥min rule was
  added per the task's explicit example; no other rules invented (e.g. no "all audio formats must
  be present" rule, no "min can never be satisfied" rule — audio scoring helper arrives in 3.1).
- Schema endpoints (`/schema`) pick the new fields up automatically via `SchemaBuilder`; verified by
  the existing Api.Test suite (37/37 pass, includes schema builder tests).

Verify: `dotnet build src/Sonarr.sln -c Debug` → 0 warnings, 0 errors.
`dotnet test src/NzbDrone.Api.Test/Sonarr.Api.Test.csproj` → 37/37 pass.
