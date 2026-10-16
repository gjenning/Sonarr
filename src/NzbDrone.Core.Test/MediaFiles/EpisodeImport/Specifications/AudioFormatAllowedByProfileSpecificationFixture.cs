using System.Collections.Generic;
using FizzWare.NBuilder;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.AudioFormats;
using NzbDrone.Core.MediaFiles.EpisodeImport;
using NzbDrone.Core.MediaFiles.EpisodeImport.Specifications;
using NzbDrone.Core.MediaFiles.MediaInfo;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.Profiles;
using NzbDrone.Core.Profiles.Qualities;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Core.Tv;

namespace NzbDrone.Core.Test.MediaFiles.EpisodeImport.Specifications
{
    [TestFixture]
    public class AudioFormatAllowedByProfileSpecificationFixture : CoreTest<AudioFormatAllowedByProfileSpecification>
    {
        private Series _series;
        private LocalEpisode _localEpisode;

        [SetUp]
        public void Setup()
        {
            _series = Builder<Series>.CreateNew()
                                     .With(s => s.SeriesType = SeriesTypes.Standard)
                                     .With(s => s.QualityProfile = new QualityProfile())
                                     .Build();

            _localEpisode = new LocalEpisode
            {
                Path = @"C:\Test\30 Rock\30.rock.s01e01.1080p.mkv",
                Series = _series,
                MediaInfo = new MediaInfoModel { AudioStreams = new List<MediaInfoAudioStreamModel>() }
            };
        }

        [Test]
        public void should_accept_when_minimum_score_is_not_set()
        {
            GivenAudioProfile(minAudioFormatScore: 0, items: new List<ProfileAudioFormatItem>());

            _localEpisode.AudioFormats = new List<AudioFormat>();

            Subject.IsSatisfiedBy(_localEpisode, null).Accepted.Should().BeTrue();
        }

        [Test]
        public void should_reject_when_audio_score_is_below_minimum()
        {
            var audioFormat = new AudioFormat { Id = 1, Name = "7.1" };

            GivenAudioProfile(minAudioFormatScore: 20, items: new List<ProfileAudioFormatItem>
            {
                new ProfileAudioFormatItem { Format = audioFormat, Score = 10 }
            });

            _localEpisode.AudioFormats = new List<AudioFormat> { audioFormat };

            var result = Subject.IsSatisfiedBy(_localEpisode, null);
            result.Accepted.Should().BeFalse();
            result.Reason.Should().Be(ImportRejectionReason.AudioFormatMinimumScore);
        }

        [Test]
        public void should_reject_when_no_audio_formats_match_and_minimum_is_set()
        {
            var audioFormat = new AudioFormat { Id = 1, Name = "7.1" };

            GivenAudioProfile(minAudioFormatScore: 10, items: new List<ProfileAudioFormatItem>
            {
                new ProfileAudioFormatItem { Format = audioFormat, Score = 10 }
            });

            _localEpisode.AudioFormats = new List<AudioFormat>();

            var result = Subject.IsSatisfiedBy(_localEpisode, null);
            result.Accepted.Should().BeFalse();
            result.Reason.Should().Be(ImportRejectionReason.AudioFormatMinimumScore);
        }

        [Test]
        public void should_accept_when_audio_score_is_equal_to_minimum()
        {
            var audioFormat = new AudioFormat { Id = 1, Name = "7.1" };

            GivenAudioProfile(minAudioFormatScore: 10, items: new List<ProfileAudioFormatItem>
            {
                new ProfileAudioFormatItem { Format = audioFormat, Score = 10 }
            });

            _localEpisode.AudioFormats = new List<AudioFormat> { audioFormat };

            Subject.IsSatisfiedBy(_localEpisode, null).Accepted.Should().BeTrue();
        }

        [Test]
        public void should_accept_when_audio_score_is_above_minimum()
        {
            var stereoFormat = new AudioFormat { Id = 1, Name = "7.1" };
            var atmosFormat = new AudioFormat { Id = 2, Name = "Atmos" };

            GivenAudioProfile(minAudioFormatScore: 10, items: new List<ProfileAudioFormatItem>
            {
                new ProfileAudioFormatItem { Format = stereoFormat, Score = 10 },
                new ProfileAudioFormatItem { Format = atmosFormat, Score = 20 }
            });

            _localEpisode.AudioFormats = new List<AudioFormat> { stereoFormat, atmosFormat };

            Subject.IsSatisfiedBy(_localEpisode, null).Accepted.Should().BeTrue();
        }

        [Test]
        public void should_accept_when_media_info_is_null()
        {
            var audioFormat = new AudioFormat { Id = 1, Name = "7.1" };

            GivenAudioProfile(minAudioFormatScore: 10, items: new List<ProfileAudioFormatItem>
            {
                new ProfileAudioFormatItem { Format = audioFormat, Score = 10 }
            });

            _localEpisode.MediaInfo = null;
            _localEpisode.AudioFormats = new List<AudioFormat>();

            Subject.IsSatisfiedBy(_localEpisode, null).Accepted.Should().BeTrue();
        }

        private void GivenAudioProfile(int minAudioFormatScore, List<ProfileAudioFormatItem> items)
        {
            var profile = _series.QualityProfile.Value;
            profile.MinAudioFormatScore = minAudioFormatScore;
            profile.AudioFormatItems = items;
        }
    }
}
