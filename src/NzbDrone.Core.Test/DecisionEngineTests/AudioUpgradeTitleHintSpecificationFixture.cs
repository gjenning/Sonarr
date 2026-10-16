using System;
using System.Collections.Generic;
using FizzWare.NBuilder;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using NzbDrone.Core.AudioFormats;
using NzbDrone.Core.CustomFormats;
using NzbDrone.Core.DecisionEngine;
using NzbDrone.Core.DecisionEngine.Specifications;
using NzbDrone.Core.Languages;
using NzbDrone.Core.MediaFiles;
using NzbDrone.Core.MediaFiles.MediaInfo;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.Profiles;
using NzbDrone.Core.Profiles.Qualities;
using NzbDrone.Core.Qualities;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Core.Tv;

namespace NzbDrone.Core.Test.DecisionEngineTests
{
    [TestFixture]

    public class AudioUpgradeTitleHintSpecificationFixture : CoreTest<AudioUpgradeTitleHintSpecification>
    {
        private RemoteEpisode _parseResult;
        private EpisodeFile _firstFile;
        private QualityProfile _profile;

        [SetUp]
        public void Setup()
        {
            Mocker.Resolve<UpgradableSpecification>();

            _firstFile = new EpisodeFile
            {
                Quality = new QualityModel(Quality.HDTV720p, new Revision(version: 2)),
                DateAdded = DateTime.Now,
                Languages = new List<Language> { Language.English },
                MediaInfo = new MediaInfoModel()
            };

            var singleEpisodeList = new List<Episode> { new Episode { EpisodeFile = _firstFile, EpisodeFileId = 1 } };

            // Quality cutoff (HDTV720p) is met by the existing file, audio cutoff (10) is not:
            // the grab is only allowed because of the audio bypass.
            _profile = new QualityProfile
            {
                Cutoff = Quality.HDTV720p.Id,
                Items = Qualities.QualityFixture.GetDefaultQualities(),
                UpgradeAllowed = true,
                UpgradeForAudio = true,
                AudioUpgradeRequiresTitleHint = true,
                CutoffAudioFormatScore = 10,
                AudioFormatItems = new List<ProfileAudioFormatItem>
                {
                    new ProfileAudioFormatItem
                    {
                        Format = new AudioFormat("DTS-HD") { Id = 1 },
                        Score = 5
                    }
                }
            };

            var fakeSeries = Builder<Series>.CreateNew()
                .With(c => c.QualityProfile = _profile)
                .Build();

            _parseResult = new RemoteEpisode
            {
                Series = fakeSeries,
                ParsedEpisodeInfo = new ParsedEpisodeInfo { Quality = new QualityModel(Quality.HDTV720p, new Revision(version: 2)), Languages = new List<Language> { Language.English } },
                Episodes = singleEpisodeList,
                CustomFormats = new List<CustomFormat>(),
                Release = new ReleaseInfo { Title = "Series.Title.S01E01.720p.HDTV.x264-Group" }
            };

            Mocker.GetMock<ICustomFormatCalculationService>()
                  .Setup(x => x.ParseCustomFormat(It.IsAny<EpisodeFile>()))
                  .Returns(new List<CustomFormat>());

            // Existing file scores 0, below the audio cutoff of 10 → audio upgrade pending.
            Mocker.GetMock<IAudioFormatCalculationService>()
                  .Setup(x => x.ParseAudioFormats(It.IsAny<EpisodeFile>()))
                  .Returns(new List<AudioFormat>());
        }

        [Test]
        public void should_reject_when_audio_upgrade_is_pending_and_title_has_no_audio_keyword()
        {
            var result = Subject.IsSatisfiedBy(_parseResult, new());

            result.Accepted.Should().BeFalse();
            result.Reason.Should().Be(DownloadRejectionReason.AudioUpgradeTitleHintMissing);
        }

        [TestCase("Series.Title.S01E01.720p.HDTV.DTS-HD.MA.5.1-Group")]
        [TestCase("Series.Title.S01E01.720p.HDTV.TrueHD.Atmos.x264-Group")]
        [TestCase("Series.Title.S01E01.720p.HDTV.AC3.5.1.x264-Group")]
        [TestCase("Series.Title.S01E01.720p.HDTV.DDPA.x264-Group")]
        [TestCase("Series.Title.S01E01.720p.HDTV.FLAC.24bit.x264-Group")]
        [TestCase("Series.Title.S01E01.720p.HDTV.x264.ATMOS-Group")]
        public void should_accept_when_audio_upgrade_is_pending_and_title_has_audio_keyword(string title)
        {
            _parseResult.Release.Title = title;

            Subject.IsSatisfiedBy(_parseResult, new()).Accepted.Should().BeTrue();
        }

        [Test]
        public void should_accept_when_title_hint_is_disabled()
        {
            _profile.AudioUpgradeRequiresTitleHint = false;

            Subject.IsSatisfiedBy(_parseResult, new()).Accepted.Should().BeTrue();
        }

        [Test]
        public void should_accept_when_upgrade_for_audio_is_disabled()
        {
            _profile.UpgradeForAudio = false;

            Subject.IsSatisfiedBy(_parseResult, new()).Accepted.Should().BeTrue();
        }

        [Test]
        public void should_accept_when_profile_has_no_audio_formats()
        {
            _profile.AudioFormatItems = new List<ProfileAudioFormatItem>();

            Subject.IsSatisfiedBy(_parseResult, new()).Accepted.Should().BeTrue();
        }

        [Test]
        public void should_accept_when_no_existing_file()
        {
            _parseResult.Episodes = new List<Episode> { new Episode { EpisodeFile = null, EpisodeFileId = 0 } };

            Subject.IsSatisfiedBy(_parseResult, new()).Accepted.Should().BeTrue();
        }

        [Test]
        public void should_accept_when_existing_file_has_no_media_info()
        {
            _firstFile.MediaInfo = null;

            Subject.IsSatisfiedBy(_parseResult, new()).Accepted.Should().BeTrue();
        }

        [Test]
        public void should_accept_when_audio_cutoff_is_already_met()
        {
            _profile.CutoffAudioFormatScore = 5;

            Mocker.GetMock<IAudioFormatCalculationService>()
                  .Setup(x => x.ParseAudioFormats(It.IsAny<EpisodeFile>()))
                  .Returns(new List<AudioFormat> { new AudioFormat("DTS-HD") { Id = 1 } });

            Subject.IsSatisfiedBy(_parseResult, new()).Accepted.Should().BeTrue();
        }

        [Test]
        public void should_accept_video_upgrade_without_audio_keyword()
        {
            _firstFile.Quality = new QualityModel(Quality.SDTV, new Revision(version: 2));

            Subject.IsSatisfiedBy(_parseResult, new()).Accepted.Should().BeTrue();
        }

        [Test]
        public void should_accept_revision_upgrade_without_audio_keyword()
        {
            _firstFile.Quality = new QualityModel(Quality.HDTV720p, new Revision(version: 1));

            Subject.IsSatisfiedBy(_parseResult, new()).Accepted.Should().BeTrue();
        }

        [Test]
        public void should_accept_when_release_is_missing()
        {
            _parseResult.Release = null;

            Subject.IsSatisfiedBy(_parseResult, new()).Accepted.Should().BeTrue();
        }
    }
}
