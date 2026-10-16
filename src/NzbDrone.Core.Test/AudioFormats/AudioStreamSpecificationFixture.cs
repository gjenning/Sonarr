using System.Collections.Generic;
using FizzWare.NBuilder;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.AudioFormats;
using NzbDrone.Core.Languages;
using NzbDrone.Core.MediaFiles.MediaInfo;
using NzbDrone.Core.Test.Framework;

namespace NzbDrone.Core.Test.AudioFormats
{
    [TestFixture]
    public class AudioStreamSpecificationFixture : CoreTest<AudioStreamSpecification>
    {
        private AudioFormatInput _input;

        [SetUp]
        public void Setup()
        {
            _input = new AudioFormatInput
            {
                MediaInfo = new MediaInfoModel
                {
                    AudioStreams = new List<MediaInfoAudioStreamModel>()
                }
            };
        }

        private MediaInfoAudioStreamModel AudioStream(
            string codec = "ac3",
            int channels = 6,
            string channelPositions = null,
            long bitrate = 640000,
            string title = "",
            string language = "eng",
            bool visualImpaired = false)
        {
            return Builder<MediaInfoAudioStreamModel>.CreateNew()
                .With(s => s.Format, codec)
                .With(s => s.CodecId, "")
                .With(s => s.Profile, "")
                .With(s => s.Channels, channels)
                .With(s => s.ChannelPositions, channelPositions)
                .With(s => s.Bitrate, bitrate)
                .With(s => s.Title, title)
                .With(s => s.Language, language)
                .With(s => s.VisualImpaired, visualImpaired)
                .Build();
        }

        private void GivenSpecification(AudioStreamField field, string value, bool negate = false)
        {
            Subject.Field = field;
            Subject.Value = value;
            Subject.Negate = negate;
            Subject.Required = true;
        }

        private void GivenStreams(params MediaInfoAudioStreamModel[] streams)
        {
            _input.MediaInfo.AudioStreams = new List<MediaInfoAudioStreamModel>(streams);
        }

        // Mirrors the per-stream group evaluation the calculation service performs in T1.6:
        // a group matches when every specification matches the same stream.
        private bool GroupMatches(params AudioStreamSpecification[] specs)
        {
            foreach (var stream in _input.MediaInfo.AudioStreams)
            {
                var allMatch = true;
                foreach (var spec in specs)
                {
                    allMatch &= spec.IsSatisfiedBy(_input, stream);
                }

                if (allMatch)
                {
                    return true;
                }
            }

            return false;
        }

        [Test]
        public void should_have_defaults()
        {
            Subject.Field.Should().Be(AudioStreamField.Codec);
            Subject.Value.Should().BeEmpty();
            Subject.Negate.Should().BeFalse();
            Subject.Required.Should().BeTrue();
            Subject.Order.Should().Be(1);
        }

        [Test]
        public void should_not_match_when_no_streams()
        {
            GivenSpecification(AudioStreamField.Codec, "truehd");
            GivenStreams();

            Subject.IsSatisfiedBy(_input).Should().BeFalse();
        }

        [Test]
        public void should_not_match_when_no_streams_and_not_required()
        {
            GivenSpecification(AudioStreamField.Codec, "truehd");
            Subject.Required = false;
            GivenStreams();

            Subject.IsSatisfiedBy(_input).Should().BeFalse();
        }

        [TestCase("truehd", true)]
        [TestCase("TRUEHD", true)]
        [TestCase("ac3", true)]
        [TestCase("dts", false)]
        [TestCase("eac3", false)]
        public void codec_should_match_stream(string value, bool expected)
        {
            GivenSpecification(AudioStreamField.Codec, value);
            GivenStreams(AudioStream(codec: "truehd"), AudioStream(codec: "ac3"));

            Subject.IsSatisfiedBy(_input).Should().Be(expected);
        }

        [TestCase("truehd", false)]
        [TestCase("dts", true)]
        public void negated_codec_should_match_stream(string value, bool expected)
        {
            GivenSpecification(AudioStreamField.Codec, value, negate: true);
            GivenStreams(AudioStream(codec: "truehd"));

            Subject.IsSatisfiedBy(_input).Should().Be(expected);
        }

        [TestCase("5.1", true)]
        [TestCase("2.0", false)]
        [TestCase("5.1|7.1", true)]
        [TestCase("^2", false)]
        public void channels_should_match_stream(string value, bool expected)
        {
            GivenSpecification(AudioStreamField.Channels, value);
            GivenStreams(AudioStream(channels: 6, channelPositions: "5.1(side)"));

            Subject.IsSatisfiedBy(_input).Should().Be(expected);
        }

        [TestCase("2.0", false)]
        [TestCase("5.1", true)]
        public void negated_channels_should_match_stream(string value, bool expected)
        {
            GivenSpecification(AudioStreamField.Channels, value, negate: true);
            GivenStreams(AudioStream(channels: 2, channelPositions: "stereo"));

            Subject.IsSatisfiedBy(_input).Should().Be(expected);
        }

        [TestCase(">800", 1000000, true)]
        [TestCase(">=1000", 1000000, true)]
        [TestCase(">1000", 1000000, false)]
        [TestCase("<1000", 1000000, false)]
        [TestCase("<=1000", 1000000, true)]
        [TestCase("=640", 640000, true)]
        [TestCase("448", 640000, true)]
        [TestCase(">=700", 640000, false)]
        [TestCase(">0", 0, false)]
        public void bitrate_should_match_stream(string value, long bitrate, bool expected)
        {
            GivenSpecification(AudioStreamField.Bitrate, value);
            GivenStreams(AudioStream(bitrate: bitrate));

            Subject.IsSatisfiedBy(_input).Should().Be(expected);
        }

        [TestCase(">800", false)]
        [TestCase(">5000", true)]
        public void negated_bitrate_should_match_stream(string value, bool expected)
        {
            GivenSpecification(AudioStreamField.Bitrate, value, negate: true);
            GivenStreams(AudioStream(bitrate: 1000000));

            Subject.IsSatisfiedBy(_input).Should().Be(expected);
        }

        [TestCase("descriptive", true)]
        [TestCase("^Commentary$", false)]
        [TestCase("(?i)DESCRIPTIVE", true)]
        public void title_should_match_stream(string value, bool expected)
        {
            GivenSpecification(AudioStreamField.Title, value);
            GivenStreams(AudioStream(title: "Extended Descriptive Mix"));

            Subject.IsSatisfiedBy(_input).Should().Be(expected);
        }

        [TestCase("commentary", false)]
        [TestCase("descriptive", true)]
        public void negated_title_should_match_stream(string value, bool expected)
        {
            GivenSpecification(AudioStreamField.Title, value, negate: true);
            GivenStreams(AudioStream(title: "Commentary"));

            Subject.IsSatisfiedBy(_input).Should().Be(expected);
        }

        [TestCase(1, true)]
        [TestCase(3, false)]
        public void language_should_match_stream(int languageId, bool expected)
        {
            GivenSpecification(AudioStreamField.Language, languageId.ToString());
            GivenStreams(AudioStream(language: "eng"));

            Subject.IsSatisfiedBy(_input).Should().Be(expected);
        }

        [Test]
        public void language_should_match_any_of_multiple_ids()
        {
            GivenSpecification(AudioStreamField.Language, $"{Language.Spanish.Id},{Language.English.Id}");
            GivenStreams(AudioStream(language: "eng"));

            Subject.IsSatisfiedBy(_input).Should().BeTrue();
        }

        [Test]
        public void negated_language_should_match_stream()
        {
            GivenSpecification(AudioStreamField.Language, Language.Spanish.Id.ToString(), negate: true);
            GivenStreams(AudioStream(language: "eng"));

            Subject.IsSatisfiedBy(_input).Should().BeTrue();

            Subject.Value = Language.English.Id.ToString();

            Subject.IsSatisfiedBy(_input).Should().BeFalse();
        }

        [TestCase("und", true)]
        [TestCase("spa", false)]
        public void language_should_match_unknown(string streamLanguage, bool expected)
        {
            GivenSpecification(AudioStreamField.Language, "0");
            GivenStreams(AudioStream(language: streamLanguage));

            Subject.IsSatisfiedBy(_input).Should().Be(expected);
        }

        [TestCase("true", true)]
        [TestCase("false", false)]
        public void audio_description_should_match_flag(string value, bool expected)
        {
            GivenSpecification(AudioStreamField.AudioDescription, value);
            GivenStreams(AudioStream(visualImpaired: true));

            Subject.IsSatisfiedBy(_input).Should().Be(expected);
        }

        [TestCase("Audio Description", true)]
        [TestCase("AD", true)]
        [TestCase("ad", true)]
        [TestCase("Audio Descriptive", true)]
        [TestCase("Commentary", false)]
        [TestCase("", false)]
        public void audio_description_should_match_title(string title, bool expected)
        {
            GivenSpecification(AudioStreamField.AudioDescription, "true");
            GivenStreams(AudioStream(title: title));

            Subject.IsSatisfiedBy(_input).Should().Be(expected);
        }

        [TestCase("true", false)]
        [TestCase("false", true)]
        public void negated_audio_description_should_match_stream(string value, bool expected)
        {
            GivenSpecification(AudioStreamField.AudioDescription, value, negate: true);
            GivenStreams(AudioStream(visualImpaired: true));

            Subject.IsSatisfiedBy(_input).Should().Be(expected);
        }

        [TestCase(">=2", true)]
        [TestCase("=3", true)]
        [TestCase("=2", false)]
        [TestCase("<3", false)]
        [TestCase("1", true)]
        public void stream_count_should_match(string value, bool expected)
        {
            GivenSpecification(AudioStreamField.StreamCount, value);
            GivenStreams(AudioStream(), AudioStream(), AudioStream());

            Subject.IsSatisfiedBy(_input).Should().Be(expected);
        }

        [TestCase("<=1", true)]
        [TestCase(">=2", false)]
        public void negated_stream_count_should_match(string value, bool expected)
        {
            GivenSpecification(AudioStreamField.StreamCount, value, negate: true);
            GivenStreams(AudioStream(), AudioStream());

            Subject.IsSatisfiedBy(_input).Should().Be(expected);
        }

        [Test]
        public void group_should_match_when_one_stream_satisfies_all_specifications()
        {
            // Group semantics (enforced by the calculator in T1.6): every specification in a
            // group must hold for the same stream.
            var codecSpec = new AudioStreamSpecification { Field = AudioStreamField.Codec, Value = "truehd" };
            var notAdSpec = new AudioStreamSpecification { Field = AudioStreamField.AudioDescription, Value = "true", Negate = true };

            GivenStreams(
                AudioStream(codec: "truehd", visualImpaired: true),
                AudioStream(codec: "truehd", visualImpaired: false),
                AudioStream(codec: "ac3"));

            GroupMatches(codecSpec, notAdSpec).Should().BeTrue();
        }

        [Test]
        public void group_should_not_match_when_only_audio_description_stream_is_truehd()
        {
            var codecSpec = new AudioStreamSpecification { Field = AudioStreamField.Codec, Value = "truehd" };
            var notAdSpec = new AudioStreamSpecification { Field = AudioStreamField.AudioDescription, Value = "true", Negate = true };

            GivenStreams(
                AudioStream(codec: "truehd", visualImpaired: true),
                AudioStream(codec: "ac3"));

            GroupMatches(codecSpec, notAdSpec).Should().BeFalse();
        }

        private void ThenValueShouldBeValid()
        {
            var result = new AudioStreamSpecificationValidator().Validate(Subject);

            result.IsValid.Should().BeTrue();
        }

        private void ThenValueShouldBeInvalid()
        {
            var result = new AudioStreamSpecificationValidator().Validate(Subject);

            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(e => e.PropertyName == "Value");
        }

        [Test]
        public void validate_should_pass_for_valid_values()
        {
            GivenSpecification(AudioStreamField.Codec, "truehd");
            ThenValueShouldBeValid();

            GivenSpecification(AudioStreamField.Channels, "5.1|7.1");
            ThenValueShouldBeValid();

            GivenSpecification(AudioStreamField.Bitrate, ">=640");
            ThenValueShouldBeValid();

            GivenSpecification(AudioStreamField.Title, "(?i)descriptive");
            ThenValueShouldBeValid();

            GivenSpecification(AudioStreamField.Language, "1,3");
            ThenValueShouldBeValid();

            GivenSpecification(AudioStreamField.AudioDescription, "true");
            ThenValueShouldBeValid();

            GivenSpecification(AudioStreamField.StreamCount, ">=2");
            ThenValueShouldBeValid();
        }

        [Test]
        public void validate_should_fail_for_empty_value()
        {
            GivenSpecification(AudioStreamField.Codec, "");

            ThenValueShouldBeInvalid();
        }

        [Test]
        public void validate_should_fail_for_invalid_codec_regex()
        {
            GivenSpecification(AudioStreamField.Codec, "[");

            ThenValueShouldBeInvalid();
        }

        [Test]
        public void validate_should_fail_for_invalid_channels_regex()
        {
            GivenSpecification(AudioStreamField.Channels, "[");

            ThenValueShouldBeInvalid();
        }

        [TestCase("")]
        [TestCase("abc")]
        [TestCase(">abc")]
        [TestCase(">=-1")]
        public void validate_should_fail_for_invalid_bitrate(string value)
        {
            GivenSpecification(AudioStreamField.Bitrate, value);

            ThenValueShouldBeInvalid();
        }

        [Test]
        public void validate_should_fail_for_invalid_title_regex()
        {
            GivenSpecification(AudioStreamField.Title, "[");

            ThenValueShouldBeInvalid();
        }

        [TestCase("")]
        [TestCase("abc")]
        [TestCase("1,abc")]
        [TestCase("999999")]
        public void validate_should_fail_for_invalid_language(string value)
        {
            GivenSpecification(AudioStreamField.Language, value);

            ThenValueShouldBeInvalid();
        }

        [TestCase("")]
        [TestCase("maybe")]
        public void validate_should_fail_for_invalid_audio_description(string value)
        {
            GivenSpecification(AudioStreamField.AudioDescription, value);

            ThenValueShouldBeInvalid();
        }

        [TestCase("")]
        [TestCase("abc")]
        [TestCase(">=abc")]
        public void validate_should_fail_for_invalid_stream_count(string value)
        {
            GivenSpecification(AudioStreamField.StreamCount, value);

            ThenValueShouldBeInvalid();
        }
    }
}
