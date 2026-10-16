using System.Collections.Generic;
using System.Linq;
using FizzWare.NBuilder;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.AudioFormats;
using NzbDrone.Core.Datastore;
using NzbDrone.Core.MediaFiles;
using NzbDrone.Core.MediaFiles.MediaInfo;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.Profiles;
using NzbDrone.Core.Profiles.Qualities;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Core.Tv;

namespace NzbDrone.Core.Test.AudioFormats
{
    [TestFixture]
    public class AudioFormatCalculationServiceFixture : CoreTest<AudioFormatCalculationService>
    {
        private MediaInfoModel _mediaInfo;

        [SetUp]
        public void Setup()
        {
            _mediaInfo = new MediaInfoModel
            {
                AudioStreams = new List<MediaInfoAudioStreamModel>()
            };

            Mocker.GetMock<IAudioFormatService>()
                .Setup(s => s.All())
                .Returns(new List<AudioFormat>());
        }

        private MediaInfoAudioStreamModel AudioStream(
            string codec = "ac3",
            int channels = 6,
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
                .With(s => s.ChannelPositions, (string)null)
                .With(s => s.Bitrate, bitrate)
                .With(s => s.Title, title)
                .With(s => s.Language, language)
                .With(s => s.VisualImpaired, visualImpaired)
                .Build();
        }

        private AudioStreamSpecification Spec(AudioStreamField field, string value, bool negate = false)
        {
            return new AudioStreamSpecification
            {
                Field = field,
                Value = value,
                Negate = negate,
                Required = true
            };
        }

        private void GivenStreams(params MediaInfoAudioStreamModel[] streams)
        {
            _mediaInfo.AudioStreams = new List<MediaInfoAudioStreamModel>(streams);
        }

        private void GivenFormats(params AudioFormat[] formats)
        {
            Mocker.GetMock<IAudioFormatService>()
                .Setup(s => s.All())
                .Returns(formats.ToList());
        }

        private List<AudioFormat> WhenParsed()
        {
            return Subject.ParseAudioFormats(_mediaInfo, "Show.Title.S01E01.1080p");
        }

        [Test]
        public void should_return_empty_when_media_info_is_null()
        {
            GivenFormats(new AudioFormat("TrueHD", Spec(AudioStreamField.Codec, "truehd")));

            Subject.ParseAudioFormats(null, "Show.Title.S01E01").Should().BeEmpty();
        }

        [Test]
        public void should_not_match_format_without_specifications()
        {
            GivenStreams(AudioStream());
            GivenFormats(new AudioFormat("Empty"));

            WhenParsed().Should().BeEmpty();
        }

        [Test]
        public void should_match_single_specification()
        {
            GivenStreams(AudioStream(codec: "truehd"));
            GivenFormats(new AudioFormat("TrueHD", Spec(AudioStreamField.Codec, "truehd")));

            WhenParsed().Select(f => f.Name).Should().Equal("TrueHD");
        }

        [Test]
        public void should_not_match_when_no_stream_matches()
        {
            GivenStreams(AudioStream(codec: "ac3"));
            GivenFormats(new AudioFormat("TrueHD", Spec(AudioStreamField.Codec, "truehd")));

            WhenParsed().Should().BeEmpty();
        }

        [Test]
        public void should_match_group_when_one_stream_satisfies_all_specs()
        {
            GivenStreams(
                AudioStream(codec: "ac3", language: "spa"),
                AudioStream(codec: "truehd", language: "eng"));

            GivenFormats(new AudioFormat("TrueHD English",
                Spec(AudioStreamField.Codec, "truehd"),
                Spec(AudioStreamField.Language, "1")));

            WhenParsed().Should().HaveCount(1);
        }

        [Test]
        public void should_not_match_group_when_specs_match_different_streams()
        {
            GivenStreams(
                AudioStream(codec: "truehd", language: "eng"),
                AudioStream(codec: "ac3", language: "spa"));

            GivenFormats(new AudioFormat("TrueHD Spanish",
                Spec(AudioStreamField.Codec, "truehd"),
                Spec(AudioStreamField.Language, "3")));

            WhenParsed().Should().BeEmpty();
        }

        [Test]
        public void should_match_negated_spec_on_another_stream()
        {
            // [Codec~truehd, NOT AD]: the TrueHD stream is audio-described, the other
            // stream is not but is not TrueHD, so nothing satisfies the group.
            GivenStreams(
                AudioStream(codec: "truehd", visualImpaired: true),
                AudioStream(codec: "ac3"));

            GivenFormats(new AudioFormat("TrueHD not AD",
                Spec(AudioStreamField.Codec, "truehd"),
                Spec(AudioStreamField.AudioDescription, "false")));

            WhenParsed().Should().BeEmpty();
        }

        [Test]
        public void should_match_negated_spec_when_stream_satisfies_all()
        {
            GivenStreams(
                AudioStream(codec: "truehd", visualImpaired: true),
                AudioStream(codec: "truehd"));

            GivenFormats(new AudioFormat("TrueHD not AD",
                Spec(AudioStreamField.Codec, "truehd"),
                Spec(AudioStreamField.AudioDescription, "false")));

            WhenParsed().Should().HaveCount(1);
        }

        [Test]
        public void should_match_negated_spec_when_another_stream_satisfies_group()
        {
            // [NOT Codec~truehd, Channels=2]: the stereo AC3 stream satisfies both
            // (not TrueHD, 2 channels), so the group matches.
            GivenStreams(
                AudioStream(codec: "truehd", channels: 8),
                AudioStream(codec: "ac3", channels: 2));

            GivenFormats(new AudioFormat("Stereo not TrueHD",
                Spec(AudioStreamField.Codec, "truehd", negate: true),
                Spec(AudioStreamField.Channels, "^2\\.0$")));

            WhenParsed().Should().HaveCount(1);
        }

        [Test]
        public void should_not_match_negated_spec_when_only_matching_stream_fails_it()
        {
            GivenStreams(AudioStream(codec: "truehd", channels: 8));

            GivenFormats(new AudioFormat("Stereo not TrueHD",
                Spec(AudioStreamField.Codec, "truehd", negate: true),
                Spec(AudioStreamField.Channels, "^8\\.0$")));

            WhenParsed().Should().BeEmpty();
        }

        [Test]
        public void should_evaluate_stream_count_on_the_stream_set()
        {
            GivenStreams(
                AudioStream(codec: "ac3"),
                AudioStream(codec: "dts"));

            GivenFormats(new AudioFormat("Two Streams",
                Spec(AudioStreamField.StreamCount, "=2"),
                Spec(AudioStreamField.Codec, "dts")));

            WhenParsed().Should().HaveCount(1);
        }

        [Test]
        public void should_not_match_when_stream_count_fails()
        {
            GivenStreams(
                AudioStream(codec: "ac3"),
                AudioStream(codec: "dts"));

            GivenFormats(new AudioFormat("Three Streams",
                Spec(AudioStreamField.StreamCount, "=3"),
                Spec(AudioStreamField.Codec, "dts")));

            WhenParsed().Should().BeEmpty();
        }

        [Test]
        public void should_match_stream_count_zero_when_no_streams()
        {
            GivenStreams();

            GivenFormats(new AudioFormat("No Audio", Spec(AudioStreamField.StreamCount, "=0")));

            WhenParsed().Should().HaveCount(1);
        }

        [Test]
        public void should_return_matches_ordered_by_name()
        {
            GivenStreams(AudioStream(codec: "truehd"));

            GivenFormats(
                new AudioFormat("Zulu", Spec(AudioStreamField.Codec, "truehd")),
                new AudioFormat("No Match", Spec(AudioStreamField.Codec, "flac")),
                new AudioFormat("Alpha", Spec(AudioStreamField.Codec, "truehd")));

            WhenParsed().Select(f => f.Name).Should().Equal("Alpha", "Zulu");
        }

        [Test]
        public void should_use_scene_name_for_episode_file()
        {
            GivenStreams(AudioStream(title: "Surround 5.1"));

            var episodeFile = Builder<EpisodeFile>.CreateNew()
                .With(f => f.MediaInfo, _mediaInfo)
                .With(f => f.SceneName, "Show.Title.S01E01")
                .With(f => f.OriginalFilePath, (string)null)
                .With(f => f.RelativePath, (string)null)
                .Build();

            GivenFormats(new AudioFormat("Title Match", Spec(AudioStreamField.Title, "Surround")));

            Subject.ParseAudioFormats(episodeFile).Should().HaveCount(1);
        }

        [Test]
        public void should_fall_back_to_original_file_path_for_episode_file()
        {
            GivenStreams(AudioStream(codec: "truehd"));

            var episodeFile = Builder<EpisodeFile>.CreateNew()
                .With(f => f.MediaInfo, _mediaInfo)
                .With(f => f.SceneName, "")
                .With(f => f.OriginalFilePath, "/downloads/show/Show.Title.S01E01.TrueHD.mkv")
                .With(f => f.RelativePath, (string)null)
                .Build();

            GivenFormats(new AudioFormat("Codec Match", Spec(AudioStreamField.Codec, "truehd")));

            Subject.ParseAudioFormats(episodeFile).Should().HaveCount(1);
        }

        [Test]
        public void should_return_empty_for_episode_file_without_media_info()
        {
            var episodeFile = Builder<EpisodeFile>.CreateNew()
                .With(f => f.MediaInfo, null)
                .With(f => f.SceneName, "Show.Title.S01E01")
                .With(f => f.OriginalFilePath, (string)null)
                .With(f => f.RelativePath, (string)null)
                .Build();

            GivenFormats(new AudioFormat("TrueHD", Spec(AudioStreamField.Codec, "truehd")));

            Subject.ParseAudioFormats(episodeFile).Should().BeEmpty();
        }

        [Test]
        public void should_use_scene_name_for_local_episode()
        {
            GivenStreams(AudioStream(codec: "truehd"));

            var localEpisode = Builder<LocalEpisode>.CreateNew()
                .With(e => e.MediaInfo, _mediaInfo)
                .With(e => e.SceneName, "Show.Title.S01E01")
                .With(e => e.Path, "/series/show/s01e01.mkv")
                .Build();

            GivenFormats(new AudioFormat("TrueHD", Spec(AudioStreamField.Codec, "truehd")));

            Subject.ParseAudioFormats(localEpisode).Should().HaveCount(1);
        }

        [Test]
        public void should_return_empty_for_local_episode_without_media_info()
        {
            var localEpisode = Builder<LocalEpisode>.CreateNew()
                .With(e => e.MediaInfo, null)
                .With(e => e.SceneName, "Show.Title.S01E01")
                .With(e => e.Path, "/series/show/s01e01.mkv")
                .Build();

            GivenFormats(new AudioFormat("TrueHD", Spec(AudioStreamField.Codec, "truehd")));

            Subject.ParseAudioFormats(localEpisode).Should().BeEmpty();
        }

        [Test]
        public void should_update_local_episode_formats_and_score()
        {
            GivenStreams(AudioStream(codec: "truehd"));

            var format = new AudioFormat("TrueHD", Spec(AudioStreamField.Codec, "truehd"));
            GivenFormats(format);

            var profile = new QualityProfile
            {
                AudioFormatItems = new List<ProfileAudioFormatItem>
                {
                    new ProfileAudioFormatItem { Format = format, Score = 10 }
                }
            };

            var localEpisode = Builder<LocalEpisode>.CreateNew()
                .With(e => e.MediaInfo, _mediaInfo)
                .With(e => e.SceneName, "Show.Title.S01E01")
                .With(e => e.Path, "/series/show/s01e01.mkv")
                .With(e => e.Series, Builder<Series>.CreateNew()
                    .With(s => s.QualityProfile, new LazyLoaded<QualityProfile>(profile))
                    .Build())
                .Build();

            Subject.UpdateEpisodeAudioFormats(localEpisode);

            localEpisode.AudioFormats.Select(f => f.Name).Should().Equal("TrueHD");
            localEpisode.AudioFormatScore.Should().Be(10);
        }

        [Test]
        public void should_update_local_episode_with_empty_formats_without_media_info()
        {
            var localEpisode = Builder<LocalEpisode>.CreateNew()
                .With(e => e.MediaInfo, null)
                .With(e => e.SceneName, "Show.Title.S01E01")
                .With(e => e.Path, "/series/show/s01e01.mkv")
                .Build();

            GivenFormats(new AudioFormat("TrueHD", Spec(AudioStreamField.Codec, "truehd")));

            Subject.UpdateEpisodeAudioFormats(localEpisode);

            localEpisode.AudioFormats.Should().BeEmpty();
            localEpisode.AudioFormatScore.Should().Be(0);
        }
    }
}
