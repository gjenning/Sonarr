using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using FluentValidation;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Annotations;
using NzbDrone.Core.Languages;
using NzbDrone.Core.MediaFiles.MediaInfo;
using NzbDrone.Core.Parser;
using NzbDrone.Core.Validation;

namespace NzbDrone.Core.AudioFormats
{
    public enum AudioStreamField
    {
        Codec,
        Channels,
        Bitrate,
        Title,
        Language,
        AudioDescription,
        StreamCount
    }

    public class AudioStreamSpecificationValidator : AbstractValidator<AudioStreamSpecification>
    {
        public AudioStreamSpecificationValidator()
        {
            RuleFor(c => c.Field).IsInEnum();
            RuleFor(c => c.Value).NotEmpty();

            RuleFor(c => c).Custom((specification, context) =>
            {
                if (!AudioStreamSpecification.IsValidValue(specification.Field, specification.Value))
                {
                    context.AddFailure("Value", $"Invalid value '{specification.Value}' for field {specification.Field}");
                }
            });
        }
    }

    public partial class AudioStreamSpecification : IAudioFormatSpecification
    {
        private static readonly AudioStreamSpecificationValidator Validator = new AudioStreamSpecificationValidator();

        // Numeric fields (Bitrate, StreamCount) use a numeric comparison instead of a regex:
        // an optional operator prefix (>=, <=, >, <, =) followed by a number; a bare number
        // means >=. Bitrate is compared in kbps (MediaInfo stores the bitrate in bps),
        // StreamCount is compared against the total number of audio streams in the file.
        private static readonly string[] ComparisonOperators = { ">=", "<=", ">", "<", "=" };

        // A stream is treated as audio-described when ffprobe flagged it visual impaired or
        // its title looks like an audio-description track ("Audio Description", "AD",
        // "Descriptive Audio", ...).
        [GeneratedRegex(@"\b(ad|audio\s*descri\w*)\b", RegexOptions.IgnoreCase)]
        private static partial Regex AudioDescriptionRegex();

        public int Order => 1;
        public string InfoLink => "https://wiki.servarr.com/sonarr/settings";
        public string ImplementationName => "Audio Stream";

        public string Name { get; set; }
        public bool Negate { get; set; }
        public bool Required { get; set; } = true;

        [FieldDefinition(1, Label = "Field", Type = FieldType.Select, SelectOptions = typeof(AudioStreamField))]
        public AudioStreamField Field { get; set; }

        [FieldDefinition(2, Label = "Value", HelpText = "Exact value or comma-separated list for Codec/Channels (regex is still supported), comma-separated Language ids, true/false for AudioDescription, numeric comparison (>=, <=, >, <, =) for Bitrate (kbps) and StreamCount")]
        public string Value { get; set; } = "";

        public IAudioFormatSpecification Clone()
        {
            return (IAudioFormatSpecification)MemberwiseClone();
        }

        public NzbDroneValidationResult Validate()
        {
            return new NzbDroneValidationResult(Validator.Validate(this));
        }

        // Standalone semantics: matches when at least one audio stream satisfies this spec
        // (Negate applies per stream). The group semantics of a full audio format (all specs
        // must pass on the same stream) live in the calculation service; use the per-stream
        // overload below for that.
        public bool IsSatisfiedBy(AudioFormatInput input)
        {
            if (input?.MediaInfo == null)
            {
                return false;
            }

            if (Field == AudioStreamField.StreamCount)
            {
                return ApplyNegate(MatchesStreamSet(input));
            }

            var streams = input.MediaInfo.AudioStreams;

            return streams != null && streams.Any(stream => ApplyNegate(Matches(input, stream)));
        }

        // Per-stream evaluation for the calculation service: evaluates this spec against a
        // single stream, applying Negate. StreamCount is the documented exception: it is
        // evaluated against the whole stream set, so the stream argument is ignored there.
        public bool IsSatisfiedBy(AudioFormatInput input, MediaInfoAudioStreamModel stream)
        {
            if (input?.MediaInfo == null)
            {
                return false;
            }

            if (Field == AudioStreamField.StreamCount)
            {
                return ApplyNegate(MatchesStreamSet(input));
            }

            return ApplyNegate(Matches(input, stream));
        }

        internal static bool IsValidValue(AudioStreamField field, string value)
        {
            switch (field)
            {
                case AudioStreamField.Codec:
                case AudioStreamField.Channels:
                case AudioStreamField.Title:
                    return value.IsNotNullOrWhiteSpace() && IsValidRegex(value);

                case AudioStreamField.Bitrate:
                case AudioStreamField.StreamCount:
                    return TryParseComparison(value, out _, out _);

                case AudioStreamField.Language:
                    return ParseLanguageIds(value, strict: true);

                case AudioStreamField.AudioDescription:
                    return bool.TryParse(value, out _);

                default:
                    return false;
            }
        }

        internal static bool TryParseComparison(string value, out string op, out double number)
        {
            op = ">=";
            number = 0;

            if (value.IsNullOrWhiteSpace())
            {
                return false;
            }

            var trimmed = value.Trim();

            foreach (var candidate in ComparisonOperators)
            {
                if (trimmed.StartsWith(candidate))
                {
                    op = candidate;
                    trimmed = trimmed[candidate.Length..].Trim();
                    break;
                }
            }

            if (!double.TryParse(trimmed, NumberStyles.Float, CultureInfo.InvariantCulture, out number))
            {
                return false;
            }

            return number >= 0;
        }

        private static bool ParseLanguageIds(string value, bool strict)
        {
            if (value.IsNullOrWhiteSpace())
            {
                return false;
            }

            var parts = value.Split(',', StringSplitOptions.RemoveEmptyEntries)
                             .Select(part => part.Trim())
                             .Where(part => part.IsNotNullOrWhiteSpace())
                             .ToList();

            if (parts.Count == 0)
            {
                return false;
            }

            return parts.All(part => int.TryParse(part, out var id) &&
                                     (strict ? Language.All.Any(l => l.Id == id) : true));
        }

        private static List<int> GetLanguageIds(string value)
        {
            if (value.IsNullOrWhiteSpace())
            {
                return new List<int>();
            }

            return value.Split(',', StringSplitOptions.RemoveEmptyEntries)
                        .Select(part => part.Trim())
                        .Where(part => int.TryParse(part, out _))
                        .Select(int.Parse)
                        .ToList();
        }

        private static bool IsValidRegex(string pattern)
        {
            try
            {
                _ = new Regex(pattern);
                return true;
            }
            catch (ArgumentException)
            {
                return false;
            }
        }

        private static bool RegexMatches(string pattern, string value)
        {
            if (pattern.IsNullOrWhiteSpace() || value == null)
            {
                return false;
            }

            try
            {
                return Regex.IsMatch(value, pattern, RegexOptions.IgnoreCase);
            }
            catch (ArgumentException)
            {
                return false;
            }
        }

        // A value whose comma-separated tokens are all known options (see
        // AudioStreamValueOptions) is matched as an exact, case-insensitive list; anything
        // else keeps regex semantics so hand-written conditions keep working. Returns null
        // when the value is not a pure option list, meaning the caller should fall back to
        // regex matching.
        private static bool? MatchesOptionList(string value, List<string> options, string actual)
        {
            var tokens = value.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

            if (tokens.Length == 0 ||
                !tokens.All(token => options.Any(option => string.Equals(option, token, StringComparison.OrdinalIgnoreCase))))
            {
                return null;
            }

            return tokens.Any(token => string.Equals(token, actual, StringComparison.OrdinalIgnoreCase));
        }

        private static bool NumericCompare(string value, double actual)
        {
            if (!TryParseComparison(value, out var op, out var number))
            {
                return false;
            }

            return op switch
            {
                ">=" => actual >= number,
                "<=" => actual <= number,
                ">" => actual > number,
                "<" => actual < number,
                "=" => actual == number,
                _ => false
            };
        }

        private static bool LanguageMatches(string value, string streamLanguage)
        {
            var ids = GetLanguageIds(value);

            if (ids.Count == 0)
            {
                return false;
            }

            var language = streamLanguage.IsNullOrWhiteSpace()
                ? Language.Unknown
                : IsoLanguages.Find(streamLanguage)?.Language ?? Language.Unknown;

            return ids.Any(id => Language.All.Any(l => l.Id == id) && (Language)id == language);
        }

        private static bool IsAudioDescribed(MediaInfoAudioStreamModel stream)
        {
            return stream.VisualImpaired ||
                   (stream.Title.IsNotNullOrWhiteSpace() && AudioDescriptionRegex().IsMatch(stream.Title));
        }

        private bool ApplyNegate(bool match)
        {
            return Negate ? !match : match;
        }

        private bool MatchesStreamSet(AudioFormatInput input)
        {
            // StreamCount is evaluated on the stream set, not per stream.
            return NumericCompare(Value, input.MediaInfo.AudioStreams?.Count ?? 0);
        }

        private bool Matches(AudioFormatInput input, MediaInfoAudioStreamModel stream)
        {
            if (stream == null)
            {
                return false;
            }

            switch (Field)
            {
                case AudioStreamField.Codec:
                {
                    var codec = MediaInfoFormatter.FormatAudioCodec(stream, input.SceneName);

                    return MatchesOptionList(Value, AudioStreamValueOptions.CodecOptions, codec) ?? RegexMatches(Value, codec);
                }

                case AudioStreamField.Channels:
                {
                    var channels = MediaInfoFormatter.FormatAudioChannels(stream).ToString("0.0", CultureInfo.InvariantCulture);

                    return MatchesOptionList(Value, AudioStreamValueOptions.ChannelOptions, channels) ?? RegexMatches(Value, channels);
                }

                case AudioStreamField.Bitrate:
                    return NumericCompare(Value, stream.Bitrate / 1000.0);

                case AudioStreamField.Title:
                    return RegexMatches(Value, stream.Title);

                case AudioStreamField.Language:
                    return LanguageMatches(Value, stream.Language);

                case AudioStreamField.AudioDescription:
                    return bool.TryParse(Value, out var expected) && IsAudioDescribed(stream) == expected;

                default:
                    return false;
            }
        }
    }
}
