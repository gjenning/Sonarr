# Task 1.5: AudioStreamSpecification (all fields)

Status: DONE
Phase: 1
Depends: 1.2
Verify: `dotnet test src/NzbDrone.Core.Test -c Debug -p:SolutionDir=$PWD/src/ --filter "FullyQualifiedName~AudioFormats"`

## Goal
The single generic specification that matches MediaInfo audio streams: `AudioStreamSpecification`
with a `Field` selector — `Codec, Channels, Bitrate, Title, Language, AudioDescription,
StreamCount` — plus unit tests. This is the heart of the feature.

## Context
Matching semantics (locked decision): **an audio format matches iff at least one audio stream
satisfies every spec in the group** (each spec's `Negate` applies per-stream). So
`[Codec~truehd, Lang=eng, NOT AudioDescription]` matches iff some stream is non-AD English
TrueHD.

Field semantics:
- `Codec` — regex vs `MediaInfoFormatter.FormatAudioCodec(stream, input.SceneName)` strings
  ("TrueHD Atmos", "EAC3 Atmos", "DTS-HD MA", "AC3", "FLAC", …).
- `Channels` — regex vs `MediaInfoFormatter.FormatAudioChannels(stream)` ("5.1", "7.1", "2.0").
- `Bitrate` — numeric compare (`>=`, `<=`, `=` operator selector or regex on kbps int; pick one,
  document in code).
- `Title` — regex vs `stream.Title`.
- `Language` — Sonarr `Language` id(s) vs `stream.Languages` (see how
  `CustomFormats/Specifications/LanguageSpecification.cs` handles include/exclude + unknown).
- `AudioDescription` — bool: stream is AD if `stream.VisualImpaired` (task 1.1) OR
  `stream.Title` matches `(?i)\b(ad|audio\s*descri\w*)\b`.
- `StreamCount` — numeric compare on total audio stream count (evaluated on the stream set, not
  per-stream; document this exception).

Mirror the CF spec pattern for schema metadata: `[FieldDefinition]`/`Enum` inputs so the API
schema endpoint (2.3) can render it. Look at
`src/NzbDrone.Core/CustomFormats/Specifications/RegexSpecification.cs` and
`IndexerFlagsSpecification.cs` for value-type + enum-field patterns.

## Files
- Create: `src/NzbDrone.Core/AudioFormats/Specifications/AudioStreamSpecification.cs`
  (+ small enum types in same file or `AudioStreamField.cs`)
- Test: `src/NzbDrone.Core.Test/AudioFormats/AudioStreamSpecificationFixture.cs`

## Steps
1. Define `AudioStreamField` enum + `AudioStreamSpecification : IAudioFormatSpecification` with
   `Field`, `Value` (string), `Negate`, `Required`, `Order`.
2. Implement per-field evaluation in `IsSatisfiedBy(AudioFormatInput)`: iterate
   `input.MediaInfo.AudioStreams`, all active specs must pass on one stream (the group logic
   lives in 1.6's calculator — this spec evaluates itself per-stream; expose a
   `IsSatisfiedBy(AudioFormatInput, MediaInfoAudioStreamModel)` overload for the calculator).
3. Validation: regex fields validate the regex on save (like CF regex specs do).
4. Unit tests: each field positive + negated; multi-stream group scenario (TrueHD AD + TrueHD
   non-AD + AC3 → `[Codec~truehd, NOT AD]` matches); AD via title regex; StreamCount.

## Acceptance Criteria
- [x] All 7 fields implemented with documented compare semantics.
- [x] Group/negate semantics covered by tests; tests pass.

## Result
Created `src/NzbDrone.Core/AudioFormats/Specifications/AudioStreamSpecification.cs`:
- `AudioStreamField` enum (Codec, Channels, Bitrate, Title, Language, AudioDescription, StreamCount).
- `AudioStreamSpecification : IAudioFormatSpecification` with `Field`, `Value` (string, default ""),
  `Negate`, `Required` (default true), `Order` (1), `[FieldDefinition]` metadata for the API schema endpoint,
  and `Clone()`.
- `IsSatisfiedBy(AudioFormatInput)`: existential over streams with per-stream negate; StreamCount is
  evaluated on the stream set (documented exception).
- `IsSatisfiedBy(AudioFormatInput, MediaInfoAudioStreamModel)` per-stream overload for the 1.6 calculator.
- Field semantics (documented in code): Codec/Channels/Title = case-insensitive regex (Channels vs
  `MediaInfoFormatter.FormatAudioChannels` formatted "0.0"); Bitrate = numeric compare in kbps with
  operator prefix (`>=`, `<=`, `>`, `<`, `=`; bare number means `>=`); Language = comma-separated
  Language ids vs stream ISO code via `IsoLanguages`; AudioDescription = "true"/"false" vs
  `VisualImpaired` OR title regex `(?i)\b(ad|audio\s*descri\w*)\b`; StreamCount = numeric compare on
  total stream count.
- `AudioStreamSpecificationValidator` (FluentValidation): Value NotEmpty + per-field value validation
  (regex compiles, numeric comparison parses and is non-negative, language ids exist, bool for AD).

Test `src/NzbDrone.Core.Test/AudioFormats/AudioStreamSpecificationFixture.cs`: 75 test cases — each
field positive + negated, no-streams cases, multi-stream group scenario (TrueHD AD + TrueHD non-AD +
AC3 with `[Codec~truehd, NOT AD]`), AD title regex cases, and validator pass/fail cases per field.

Verify: `dotnet test src/NzbDrone.Core.Test -c Debug -p:SolutionDir=$PWD/src/ --filter "FullyQualifiedName~AudioFormats"`
→ Passed: 75, Failed: 0.

Deviations from the task file:
- Task referenced `RegexSpecification.cs`/`IndexerFlagsSpecification.cs`; actual files are
  `RegexSpecificationBase.cs`/`IndexerFlagSpecification.cs` — mirrored those instead.
- Task said `stream.Languages`; `MediaInfoAudioStreamModel.Language` is a single ISO-code string, so
  the spec maps it with `IsoLanguages.Find(...)?.Language ?? Language.Unknown`.
- Bitrate compare: chose operator-prefix numeric compare (task allowed picking one and documenting).
- Validator tests assert on `Validate()` results directly because `FluentValidation.TestHelper`
  extensions are not referenced by NzbDrone.Core.Test.
