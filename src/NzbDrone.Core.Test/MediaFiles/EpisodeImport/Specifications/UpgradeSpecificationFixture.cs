using System.Collections.Generic;
using System.Linq;
using FizzWare.NBuilder;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using NzbDrone.Core.AudioFormats;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.CustomFormats;
using NzbDrone.Core.Datastore;
using NzbDrone.Core.Languages;
using NzbDrone.Core.MediaFiles;
using NzbDrone.Core.MediaFiles.EpisodeImport;
using NzbDrone.Core.MediaFiles.EpisodeImport.Specifications;
using NzbDrone.Core.MediaFiles.MediaInfo;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.Profiles;
using NzbDrone.Core.Profiles.Qualities;
using NzbDrone.Core.Qualities;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Core.Tv;

namespace NzbDrone.Core.Test.MediaFiles.EpisodeImport.Specifications
{
    [TestFixture]
    public class UpgradeSpecificationFixture : CoreTest<UpgradeSpecification>
    {
        private Series _series;
        private LocalEpisode _localEpisode;

        [SetUp]
        public void Setup()
        {
            _series = Builder<Series>.CreateNew()
                                     .With(s => s.SeriesType = SeriesTypes.Standard)
                                     .With(e => e.QualityProfile = new QualityProfile
                                        {
                                            Items = Qualities.QualityFixture.GetDefaultQualities(),
                                        })
                                     .Build();

            _localEpisode = new LocalEpisode
                                {
                                    Path = @"C:\Test\30 Rock\30.rock.s01e01.avi",
                                    Quality = new QualityModel(Quality.HDTV720p, new Revision(version: 1)),
                                    Languages = new List<Language> { Language.Spanish },
                                    Series = _series
                                };
        }

        [Test]
        public void should_return_true_if_no_existing_episodeFile()
        {
            _localEpisode.Episodes = Builder<Episode>.CreateListOfSize(1)
                                                     .All()
                                                     .With(e => e.EpisodeFileId = 0)
                                                     .With(e => e.EpisodeFile = null)
                                                     .Build()
                                                     .ToList();

            Subject.IsSatisfiedBy(_localEpisode, null).Accepted.Should().BeTrue();
        }

        [Test]
        public void should_return_true_if_no_existing_episodeFile_for_multi_episodes()
        {
            _localEpisode.Episodes = Builder<Episode>.CreateListOfSize(2)
                                                     .All()
                                                     .With(e => e.EpisodeFileId = 0)
                                                     .With(e => e.EpisodeFile = null)
                                                     .Build()
                                                     .ToList();

            Subject.IsSatisfiedBy(_localEpisode, null).Accepted.Should().BeTrue();
        }

        [Test]
        public void should_return_true_if_upgrade_for_existing_episodeFile()
        {
            _localEpisode.Episodes = Builder<Episode>.CreateListOfSize(1)
                                                     .All()
                                                     .With(e => e.EpisodeFileId = 1)
                                                     .With(e => e.EpisodeFile = new LazyLoaded<EpisodeFile>(
                                                                                new EpisodeFile
                                                                                {
                                                                                    Quality = new QualityModel(Quality.SDTV, new Revision(version: 1)),
                                                                                    Languages = new List<Language> { Language.Spanish }
                                                                                }))
                                                     .Build()
                                                     .ToList();

            Subject.IsSatisfiedBy(_localEpisode, null).Accepted.Should().BeTrue();
        }

        [Test]
        public void should_return_true_if_language_upgrade_for_existing_episodeFile_and_quality_is_same()
        {
            _localEpisode.Episodes = Builder<Episode>.CreateListOfSize(1)
                                                     .All()
                                                     .With(e => e.EpisodeFileId = 1)
                                                     .With(e => e.EpisodeFile = new LazyLoaded<EpisodeFile>(
                                                                                new EpisodeFile
                                                                                {
                                                                                    Quality = new QualityModel(Quality.HDTV720p, new Revision(version: 1)),
                                                                                    Languages = new List<Language> { Language.English }
                                                                                }))
                                                     .Build()
                                                     .ToList();

            Subject.IsSatisfiedBy(_localEpisode, null).Accepted.Should().BeTrue();
        }

        [Test]
        public void should_return_false_if_language_upgrade_for_existing_episodeFile_and_quality_is_worse()
        {
            _localEpisode.Episodes = Builder<Episode>.CreateListOfSize(1)
                                                     .All()
                                                     .With(e => e.EpisodeFileId = 1)
                                                     .With(e => e.EpisodeFile = new LazyLoaded<EpisodeFile>(
                                                                                new EpisodeFile
                                                                                {
                                                                                    Quality = new QualityModel(Quality.Bluray1080p, new Revision(version: 1)),
                                                                                    Languages = new List<Language> { Language.English }
                                                                                }))
                                                     .Build()
                                                     .ToList();

            Subject.IsSatisfiedBy(_localEpisode, null).Accepted.Should().BeFalse();
        }

        [Test]
        public void should_return_true_if_upgrade_for_existing_episodeFile_for_multi_episodes()
        {
            _localEpisode.Episodes = Builder<Episode>.CreateListOfSize(2)
                                                     .All()
                                                     .With(e => e.EpisodeFileId = 1)
                                                     .With(e => e.EpisodeFile = new LazyLoaded<EpisodeFile>(
                                                                                new EpisodeFile
                                                                                {
                                                                                    Quality = new QualityModel(Quality.SDTV, new Revision(version: 1))
                                                                                }))
                                                     .Build()
                                                     .ToList();

            Subject.IsSatisfiedBy(_localEpisode, null).Accepted.Should().BeTrue();
        }

        [Test]
        public void should_return_true_if_language_upgrade_for_existing_episodeFile_for_multi_episodes_and_quality_is_same()
        {
            _localEpisode.Episodes = Builder<Episode>.CreateListOfSize(2)
                                                     .All()
                                                     .With(e => e.EpisodeFileId = 1)
                                                     .With(e => e.EpisodeFile = new LazyLoaded<EpisodeFile>(
                                                                                new EpisodeFile
                                                                                {
                                                                                    Quality = new QualityModel(Quality.HDTV720p, new Revision(version: 1)),
                                                                                    Languages = new List<Language> { Language.English }
                                                                                }))
                                                     .Build()
                                                     .ToList();

            Subject.IsSatisfiedBy(_localEpisode, null).Accepted.Should().BeTrue();
        }

        [Test]
        public void should_return_false_if_language_upgrade_for_existing_episodeFile_for_multi_episodes_and_quality_is_worse()
        {
            _localEpisode.Episodes = Builder<Episode>.CreateListOfSize(2)
                                                     .All()
                                                     .With(e => e.EpisodeFileId = 1)
                                                     .With(e => e.EpisodeFile = new LazyLoaded<EpisodeFile>(
                                                                                new EpisodeFile
                                                                                {
                                                                                    Quality = new QualityModel(Quality.Bluray1080p, new Revision(version: 1)),
                                                                                    Languages = new List<Language> { Language.English }
                                                                                }))
                                                     .Build()
                                                     .ToList();

            Subject.IsSatisfiedBy(_localEpisode, null).Accepted.Should().BeFalse();
        }

        [Test]
        public void should_return_false_if_not_an_upgrade_for_existing_episodeFile()
        {
            _localEpisode.Episodes = Builder<Episode>.CreateListOfSize(1)
                                                     .All()
                                                     .With(e => e.EpisodeFileId = 1)
                                                     .With(e => e.EpisodeFile = new LazyLoaded<EpisodeFile>(
                                                                                new EpisodeFile
                                                                                {
                                                                                    Quality = new QualityModel(Quality.Bluray720p, new Revision(version: 1))
                                                                                }))
                                                     .Build()
                                                     .ToList();

            Subject.IsSatisfiedBy(_localEpisode, null).Accepted.Should().BeFalse();
        }

        [Test]
        public void should_return_false_if_not_an_upgrade_for_existing_episodeFile_for_multi_episodes()
        {
            _localEpisode.Episodes = Builder<Episode>.CreateListOfSize(2)
                                                     .All()
                                                     .With(e => e.EpisodeFileId = 1)
                                                     .With(e => e.EpisodeFile = new LazyLoaded<EpisodeFile>(
                                                                                new EpisodeFile
                                                                                {
                                                                                    Quality = new QualityModel(Quality.Bluray720p, new Revision(version: 1))
                                                                                }))
                                                     .Build()
                                                     .ToList();

            Subject.IsSatisfiedBy(_localEpisode, null).Accepted.Should().BeFalse();
        }

        [Test]
        public void should_return_false_if_not_an_upgrade_for_one_existing_episodeFile_for_multi_episode()
        {
            _localEpisode.Episodes = Builder<Episode>.CreateListOfSize(2)
                                                     .TheFirst(1)
                                                     .With(e => e.EpisodeFileId = 1)
                                                     .With(e => e.EpisodeFile = new LazyLoaded<EpisodeFile>(
                                                                                new EpisodeFile
                                                                                {
                                                                                    Quality = new QualityModel(Quality.SDTV, new Revision(version: 1))
                                                                                }))
                                                     .TheNext(1)
                                                     .With(e => e.EpisodeFileId = 2)
                                                     .With(e => e.EpisodeFile = new LazyLoaded<EpisodeFile>(
                                                                                new EpisodeFile
                                                                                {
                                                                                    Quality = new QualityModel(Quality.Bluray720p, new Revision(version: 1))
                                                                                }))
                                                     .Build()
                                                     .ToList();

            Subject.IsSatisfiedBy(_localEpisode, null).Accepted.Should().BeFalse();
        }

        [Test]
        public void should_return_false_if_not_a_revision_upgrade_and_prefers_propers()
        {
            Mocker.GetMock<IConfigService>()
                  .Setup(s => s.DownloadPropersAndRepacks)
                  .Returns(ProperDownloadTypes.PreferAndUpgrade);

            _localEpisode.Episodes = Builder<Episode>.CreateListOfSize(1)
                                                     .All()
                                                     .With(e => e.EpisodeFileId = 1)
                                                     .With(e => e.EpisodeFile = new LazyLoaded<EpisodeFile>(
                                                         new EpisodeFile
                                                         {
                                                             Quality = new QualityModel(Quality.HDTV720p, new Revision(version: 2)),
                                                             Languages = new List<Language> { Language.Spanish }
                                                         }))
                                                     .Build()
                                                     .ToList();

            Subject.IsSatisfiedBy(_localEpisode, null).Accepted.Should().BeFalse();
        }

        [Test]
        public void should_return_true_if_it_is_a_preferred_word_downgrade_and_language_downgrade_and_a_quality_upgrade()
        {
            Mocker.GetMock<IConfigService>()
                  .Setup(s => s.DownloadPropersAndRepacks)
                  .Returns(ProperDownloadTypes.DoNotPrefer);

            Mocker.GetMock<ICustomFormatCalculationService>()
                  .Setup(s => s.ParseCustomFormat(It.IsAny<EpisodeFile>()))
                  .Returns(new List<CustomFormat>());

            Mocker.GetMock<ICustomFormatCalculationService>()
                  .Setup(s => s.ParseCustomFormat(It.IsAny<RemoteEpisode>(), It.IsAny<long>()))
                  .Returns(new List<CustomFormat>());

            _localEpisode.Quality = new QualityModel(Quality.Bluray2160p);

            _localEpisode.Episodes = Builder<Episode>.CreateListOfSize(1)
                                                     .All()
                                                     .With(e => e.EpisodeFileId = 1)
                                                     .With(e => e.EpisodeFile = new LazyLoaded<EpisodeFile>(
                                                         new EpisodeFile
                                                         {
                                                             Quality = new QualityModel(Quality.Bluray1080p),
                                                             Languages = new List<Language> { Language.French }
                                                         }))
                                                     .Build()
                                                     .ToList();

            _localEpisode.FileEpisodeInfo = Builder<ParsedEpisodeInfo>.CreateNew().Build();

            Subject.IsSatisfiedBy(_localEpisode, null).Accepted.Should().BeTrue();
        }

        [Test]
        public void should_return_true_if_it_is_a_preferred_word_downgrade_but_a_language_upgrade()
        {
            Mocker.GetMock<IConfigService>()
                  .Setup(s => s.DownloadPropersAndRepacks)
                  .Returns(ProperDownloadTypes.DoNotPrefer);

            Mocker.GetMock<ICustomFormatCalculationService>()
                  .Setup(s => s.ParseCustomFormat(It.IsAny<EpisodeFile>()))
                  .Returns(new List<CustomFormat>());

            Mocker.GetMock<ICustomFormatCalculationService>()
                  .Setup(s => s.ParseCustomFormat(It.IsAny<RemoteEpisode>(), It.IsAny<long>()))
                  .Returns(new List<CustomFormat>());

            _localEpisode.Quality = new QualityModel(Quality.Bluray1080p);

            _localEpisode.Episodes = Builder<Episode>.CreateListOfSize(1)
                                                     .All()
                                                     .With(e => e.EpisodeFileId = 1)
                                                     .With(e => e.EpisodeFile = new LazyLoaded<EpisodeFile>(
                                                         new EpisodeFile
                                                         {
                                                             Quality = new QualityModel(Quality.Bluray1080p),
                                                             Languages = new List<Language> { Language.English }
                                                         }))
                                                     .Build()
                                                     .ToList();

            _localEpisode.FileEpisodeInfo = Builder<ParsedEpisodeInfo>.CreateNew().Build();

            Subject.IsSatisfiedBy(_localEpisode, null).Accepted.Should().BeTrue();
        }

        [Test]
        public void should_return_true_if_not_a_revision_upgrade_and_does_not_prefer_propers()
        {
            Mocker.GetMock<IConfigService>()
                  .Setup(s => s.DownloadPropersAndRepacks)
                  .Returns(ProperDownloadTypes.DoNotPrefer);

            _localEpisode.Episodes = Builder<Episode>.CreateListOfSize(1)
                                                     .All()
                                                     .With(e => e.EpisodeFileId = 1)
                                                     .With(e => e.EpisodeFile = new LazyLoaded<EpisodeFile>(
                                                         new EpisodeFile
                                                         {
                                                             Quality = new QualityModel(Quality.HDTV720p, new Revision(version: 2))
                                                         }))
                                                     .Build()
                                                     .ToList();

            Subject.IsSatisfiedBy(_localEpisode, null).Accepted.Should().BeTrue();
        }

        [Test]
        public void should_return_true_when_comparing_to_a_lower_quality_proper()
        {
            Mocker.GetMock<IConfigService>()
                  .Setup(s => s.DownloadPropersAndRepacks)
                  .Returns(ProperDownloadTypes.DoNotPrefer);

            _localEpisode.Quality = new QualityModel(Quality.Bluray1080p);

            _localEpisode.Episodes = Builder<Episode>.CreateListOfSize(1)
                                                     .All()
                                                     .With(e => e.EpisodeFileId = 1)
                                                     .With(e => e.EpisodeFile = new LazyLoaded<EpisodeFile>(
                                                         new EpisodeFile
                                                         {
                                                             Quality = new QualityModel(Quality.Bluray1080p, new Revision(version: 2))
                                                         }))
                                                     .Build()
                                                     .ToList();

            Subject.IsSatisfiedBy(_localEpisode, null).Accepted.Should().BeTrue();
        }

        [Test]
        public void should_return_true_if_it_is_a_preferred_word_upgrade()
        {
            Mocker.GetMock<IConfigService>()
                  .Setup(s => s.DownloadPropersAndRepacks)
                  .Returns(ProperDownloadTypes.DoNotPrefer);

            Mocker.GetMock<ICustomFormatCalculationService>()
                  .Setup(s => s.ParseCustomFormat(It.IsAny<EpisodeFile>()))
                  .Returns(new List<CustomFormat>());

            Mocker.GetMock<ICustomFormatCalculationService>()
                  .Setup(s => s.ParseCustomFormat(It.IsAny<RemoteEpisode>(), It.IsAny<long>()))
                  .Returns(new List<CustomFormat>());

            _localEpisode.Quality = new QualityModel(Quality.Bluray1080p);

            _localEpisode.Episodes = Builder<Episode>.CreateListOfSize(1)
                                                     .All()
                                                     .With(e => e.EpisodeFileId = 1)
                                                     .With(e => e.EpisodeFile = new LazyLoaded<EpisodeFile>(
                                                         new EpisodeFile
                                                         {
                                                             Quality = new QualityModel(Quality.Bluray1080p)
                                                         }))
                                                     .Build()
                                                     .ToList();

            _localEpisode.FileEpisodeInfo = Builder<ParsedEpisodeInfo>.CreateNew().Build();

            Subject.IsSatisfiedBy(_localEpisode, null).Accepted.Should().BeTrue();
        }

        [Test]
        public void should_return_true_if_it_has_an_equal_preferred_word_score()
        {
            Mocker.GetMock<IConfigService>()
                  .Setup(s => s.DownloadPropersAndRepacks)
                  .Returns(ProperDownloadTypes.DoNotPrefer);

            Mocker.GetMock<ICustomFormatCalculationService>()
                  .Setup(s => s.ParseCustomFormat(It.IsAny<EpisodeFile>()))
                  .Returns(new List<CustomFormat>());

            Mocker.GetMock<ICustomFormatCalculationService>()
                  .Setup(s => s.ParseCustomFormat(It.IsAny<RemoteEpisode>(), It.IsAny<long>()))
                  .Returns(new List<CustomFormat>());

            _localEpisode.Quality = new QualityModel(Quality.Bluray1080p);

            _localEpisode.Episodes = Builder<Episode>.CreateListOfSize(1)
                                                     .All()
                                                     .With(e => e.EpisodeFileId = 1)
                                                     .With(e => e.EpisodeFile = new LazyLoaded<EpisodeFile>(
                                                         new EpisodeFile
                                                         {
                                                             Quality = new QualityModel(Quality.Bluray1080p)
                                                         }))
                                                     .Build()
                                                     .ToList();

            _localEpisode.FileEpisodeInfo = Builder<ParsedEpisodeInfo>.CreateNew().Build();

            Subject.IsSatisfiedBy(_localEpisode, null).Accepted.Should().BeTrue();
        }

        [Test]
        public void should_return_true_if_episode_file_is_null()
        {
            _localEpisode.Episodes = Builder<Episode>.CreateListOfSize(2)
                                                     .All()
                                                     .With(e => e.EpisodeFileId = 1)
                                                     .With(e => e.EpisodeFile = new LazyLoaded<EpisodeFile>(null))
                                                     .Build()
                                                     .ToList();

            Subject.IsSatisfiedBy(_localEpisode, null).Accepted.Should().BeTrue();
        }

        [Test]
        public void should_return_true_if_upgrade_to_custom_format_score()
        {
            var episodeFileCustomFormats = Builder<CustomFormat>.CreateListOfSize(1).Build().ToList();

            var episodeFile = new EpisodeFile
            {
                Quality = new QualityModel(Quality.Bluray1080p)
            };

            _series.QualityProfile.Value.FormatItems = episodeFileCustomFormats.Select(c => new ProfileFormatItem
            {
                Format = c,
                Score = 10
            })
                .ToList();

            Mocker.GetMock<IConfigService>()
                .Setup(s => s.DownloadPropersAndRepacks)
                .Returns(ProperDownloadTypes.DoNotPrefer);

            Mocker.GetMock<ICustomFormatCalculationService>()
                .Setup(s => s.ParseCustomFormat(episodeFile))
                .Returns(episodeFileCustomFormats);

            _localEpisode.Quality = new QualityModel(Quality.Bluray1080p);
            _localEpisode.CustomFormats = Builder<CustomFormat>.CreateListOfSize(1).Build().ToList();
            _localEpisode.CustomFormatScore = 20;

            _localEpisode.Episodes = Builder<Episode>.CreateListOfSize(1)
                .All()
                .With(e => e.EpisodeFileId = 1)
                .With(e => e.EpisodeFile = new LazyLoaded<EpisodeFile>(episodeFile))
                .Build()
                .ToList();

            Subject.IsSatisfiedBy(_localEpisode, null).Accepted.Should().BeTrue();
        }

        [Test]
        public void should_return_true_if_not_upgrade_to_custom_format_score_but_is_upgrade_to_quality()
        {
            var episodeFileCustomFormats = Builder<CustomFormat>.CreateListOfSize(1).Build().ToList();

            var episodeFile = new EpisodeFile
            {
                Quality = new QualityModel(Quality.Bluray720p)
            };

            _series.QualityProfile.Value.FormatItems = episodeFileCustomFormats.Select(c => new ProfileFormatItem
                {
                    Format = c,
                    Score = 50
                })
                .ToList();

            Mocker.GetMock<IConfigService>()
                .Setup(s => s.DownloadPropersAndRepacks)
                .Returns(ProperDownloadTypes.DoNotPrefer);

            Mocker.GetMock<ICustomFormatCalculationService>()
                .Setup(s => s.ParseCustomFormat(episodeFile))
                .Returns(episodeFileCustomFormats);

            _localEpisode.Quality = new QualityModel(Quality.Bluray1080p);
            _localEpisode.CustomFormats = Builder<CustomFormat>.CreateListOfSize(1).Build().ToList();
            _localEpisode.CustomFormatScore = 20;

            _localEpisode.Episodes = Builder<Episode>.CreateListOfSize(1)
                .All()
                .With(e => e.EpisodeFileId = 1)
                .With(e => e.EpisodeFile = new LazyLoaded<EpisodeFile>(episodeFile))
                .Build()
                .ToList();

            Subject.IsSatisfiedBy(_localEpisode, null).Accepted.Should().BeTrue();
        }

        [Test]
        public void should_return_false_if_not_upgrade_to_custom_format_score()
        {
            var episodeFileCustomFormats = Builder<CustomFormat>.CreateListOfSize(1).Build().ToList();

            var episodeFile = new EpisodeFile
            {
                Quality = new QualityModel(Quality.Bluray1080p)
            };

            _series.QualityProfile.Value.FormatItems = episodeFileCustomFormats.Select(c => new ProfileFormatItem
                {
                    Format = c,
                    Score = 50
                })
                .ToList();

            Mocker.GetMock<IConfigService>()
                .Setup(s => s.DownloadPropersAndRepacks)
                .Returns(ProperDownloadTypes.DoNotPrefer);

            Mocker.GetMock<ICustomFormatCalculationService>()
                .Setup(s => s.ParseCustomFormat(episodeFile))
                .Returns(episodeFileCustomFormats);

            _localEpisode.Quality = new QualityModel(Quality.Bluray1080p);
            _localEpisode.CustomFormats = Builder<CustomFormat>.CreateListOfSize(1).Build().ToList();
            _localEpisode.CustomFormatScore = 20;

            _localEpisode.Episodes = Builder<Episode>.CreateListOfSize(1)
                .All()
                .With(e => e.EpisodeFileId = 1)
                .With(e => e.EpisodeFile = new LazyLoaded<EpisodeFile>(episodeFile))
                .Build()
                .ToList();

            Subject.IsSatisfiedBy(_localEpisode, null).Accepted.Should().BeFalse();
        }

        [Test]
        public void should_return_false_and_a_specific_reason_if_not_upgrade_to_custom_format_score_after_local_file_rename_but_was_before()
        {
            var episodeFileCustomFormats = Builder<CustomFormat>.CreateListOfSize(1).Build().ToList();

            var episodeFile = new EpisodeFile
            {
                Quality = new QualityModel(Quality.Bluray1080p)
            };

            _series.QualityProfile.Value.FormatItems = episodeFileCustomFormats.Select(c => new ProfileFormatItem
                {
                    Format = c,
                    Score = 50
                })
                .ToList();

            Mocker.GetMock<IConfigService>()
                .Setup(s => s.DownloadPropersAndRepacks)
                .Returns(ProperDownloadTypes.DoNotPrefer);

            Mocker.GetMock<ICustomFormatCalculationService>()
                .Setup(s => s.ParseCustomFormat(episodeFile))
                .Returns(episodeFileCustomFormats);

            _localEpisode.Quality = new QualityModel(Quality.Bluray1080p);
            _localEpisode.CustomFormats = Builder<CustomFormat>.CreateListOfSize(1).Build().ToList();
            _localEpisode.CustomFormatScore = 20;
            _localEpisode.OriginalFileNameCustomFormats = Builder<CustomFormat>.CreateListOfSize(1).Build().ToList();
            _localEpisode.OriginalFileNameCustomFormatScore = 60;

            _localEpisode.Episodes = Builder<Episode>.CreateListOfSize(1)
                .All()
                .With(e => e.EpisodeFileId = 1)
                .With(e => e.EpisodeFile = new LazyLoaded<EpisodeFile>(episodeFile))
                .Build()
                .ToList();

            var result = Subject.IsSatisfiedBy(_localEpisode, null);
            result.Accepted.Should().BeFalse();
            result.Reason.Should().Be(ImportRejectionReason.NotCustomFormatUpgradeAfterRename);
        }

        [Test]
        public void should_return_true_if_upgrade_to_audio_format_score()
        {
            var episodeFile = GivenCustomFormatDowngradeScenario();

            var existingAudioFormat = new AudioFormat { Id = 1, Name = "7.1" };
            var newAudioFormat = new AudioFormat { Id = 2, Name = "Atmos" };

            GivenAudioProfile(upgradeForAudio: true, cutoffAudioFormatScore: 50, items: new List<ProfileAudioFormatItem>
            {
                new ProfileAudioFormatItem { Format = existingAudioFormat, Score = 10 },
                new ProfileAudioFormatItem { Format = newAudioFormat, Score = 20 }
            });

            Mocker.GetMock<IAudioFormatCalculationService>()
                  .Setup(s => s.ParseAudioFormats(episodeFile))
                  .Returns(new List<AudioFormat> { existingAudioFormat });

            _localEpisode.AudioFormats = new List<AudioFormat> { existingAudioFormat, newAudioFormat };

            Subject.IsSatisfiedBy(_localEpisode, null).Accepted.Should().BeTrue();
        }

        [Test]
        public void should_return_false_if_audio_format_score_is_not_higher()
        {
            var episodeFile = GivenCustomFormatDowngradeScenario();

            var existingAudioFormat = new AudioFormat { Id = 1, Name = "7.1" };

            GivenAudioProfile(upgradeForAudio: true, cutoffAudioFormatScore: 50, items: new List<ProfileAudioFormatItem>
            {
                new ProfileAudioFormatItem { Format = existingAudioFormat, Score = 10 }
            });

            Mocker.GetMock<IAudioFormatCalculationService>()
                  .Setup(s => s.ParseAudioFormats(episodeFile))
                  .Returns(new List<AudioFormat> { existingAudioFormat });

            _localEpisode.AudioFormats = new List<AudioFormat> { existingAudioFormat };

            var result = Subject.IsSatisfiedBy(_localEpisode, null);
            result.Accepted.Should().BeFalse();
            result.Reason.Should().Be(ImportRejectionReason.NotCustomFormatUpgrade);
        }

        [Test]
        public void should_return_false_if_existing_audio_format_score_meets_cutoff()
        {
            var episodeFile = GivenCustomFormatDowngradeScenario();

            var existingAudioFormat = new AudioFormat { Id = 1, Name = "7.1" };
            var newAudioFormat = new AudioFormat { Id = 2, Name = "Atmos" };

            GivenAudioProfile(upgradeForAudio: true, cutoffAudioFormatScore: 10, items: new List<ProfileAudioFormatItem>
            {
                new ProfileAudioFormatItem { Format = existingAudioFormat, Score = 10 },
                new ProfileAudioFormatItem { Format = newAudioFormat, Score = 20 }
            });

            Mocker.GetMock<IAudioFormatCalculationService>()
                  .Setup(s => s.ParseAudioFormats(episodeFile))
                  .Returns(new List<AudioFormat> { existingAudioFormat });

            _localEpisode.AudioFormats = new List<AudioFormat> { existingAudioFormat, newAudioFormat };

            var result = Subject.IsSatisfiedBy(_localEpisode, null);
            result.Accepted.Should().BeFalse();
            result.Reason.Should().Be(ImportRejectionReason.NotCustomFormatUpgrade);
        }

        [Test]
        public void should_return_false_if_upgrade_for_audio_is_disabled()
        {
            var episodeFile = GivenCustomFormatDowngradeScenario();

            var existingAudioFormat = new AudioFormat { Id = 1, Name = "7.1" };
            var newAudioFormat = new AudioFormat { Id = 2, Name = "Atmos" };

            GivenAudioProfile(upgradeForAudio: false, cutoffAudioFormatScore: 50, items: new List<ProfileAudioFormatItem>
            {
                new ProfileAudioFormatItem { Format = existingAudioFormat, Score = 10 },
                new ProfileAudioFormatItem { Format = newAudioFormat, Score = 20 }
            });

            _localEpisode.AudioFormats = new List<AudioFormat> { existingAudioFormat, newAudioFormat };

            var result = Subject.IsSatisfiedBy(_localEpisode, null);
            result.Accepted.Should().BeFalse();
            result.Reason.Should().Be(ImportRejectionReason.NotCustomFormatUpgrade);

            Mocker.GetMock<IAudioFormatCalculationService>()
                  .Verify(s => s.ParseAudioFormats(It.IsAny<EpisodeFile>()), Times.Never);
        }

        [Test]
        public void should_return_false_if_no_audio_formats_on_profile()
        {
            GivenCustomFormatDowngradeScenario();

            GivenAudioProfile(upgradeForAudio: true, cutoffAudioFormatScore: 50, items: new List<ProfileAudioFormatItem>());

            _localEpisode.AudioFormats = Builder<AudioFormat>.CreateListOfSize(1).Build().ToList();

            var result = Subject.IsSatisfiedBy(_localEpisode, null);
            result.Accepted.Should().BeFalse();
            result.Reason.Should().Be(ImportRejectionReason.NotCustomFormatUpgrade);
        }

        [Test]
        public void should_return_false_if_existing_episode_file_has_no_media_info()
        {
            var episodeFile = GivenCustomFormatDowngradeScenario();
            episodeFile.MediaInfo = null;

            var existingAudioFormat = new AudioFormat { Id = 1, Name = "7.1" };
            var newAudioFormat = new AudioFormat { Id = 2, Name = "Atmos" };

            GivenAudioProfile(upgradeForAudio: true, cutoffAudioFormatScore: 50, items: new List<ProfileAudioFormatItem>
            {
                new ProfileAudioFormatItem { Format = existingAudioFormat, Score = 10 },
                new ProfileAudioFormatItem { Format = newAudioFormat, Score = 20 }
            });

            _localEpisode.AudioFormats = new List<AudioFormat> { existingAudioFormat, newAudioFormat };

            var result = Subject.IsSatisfiedBy(_localEpisode, null);
            result.Accepted.Should().BeFalse();
            result.Reason.Should().Be(ImportRejectionReason.NotCustomFormatUpgrade);
        }

        [Test]
        public void should_return_false_if_local_episode_has_no_media_info()
        {
            var episodeFile = GivenCustomFormatDowngradeScenario();
            _localEpisode.MediaInfo = null;

            var existingAudioFormat = new AudioFormat { Id = 1, Name = "7.1" };
            var newAudioFormat = new AudioFormat { Id = 2, Name = "Atmos" };

            GivenAudioProfile(upgradeForAudio: true, cutoffAudioFormatScore: 50, items: new List<ProfileAudioFormatItem>
            {
                new ProfileAudioFormatItem { Format = existingAudioFormat, Score = 10 },
                new ProfileAudioFormatItem { Format = newAudioFormat, Score = 20 }
            });

            Mocker.GetMock<IAudioFormatCalculationService>()
                  .Setup(s => s.ParseAudioFormats(episodeFile))
                  .Returns(new List<AudioFormat> { existingAudioFormat });

            _localEpisode.AudioFormats = new List<AudioFormat> { existingAudioFormat, newAudioFormat };

            var result = Subject.IsSatisfiedBy(_localEpisode, null);
            result.Accepted.Should().BeFalse();
            result.Reason.Should().Be(ImportRejectionReason.NotCustomFormatUpgrade);
        }

        [Test]
        public void should_return_false_if_video_quality_is_downgrade_even_with_audio_upgrade()
        {
            var episodeFile = GivenCustomFormatDowngradeScenario();

            var existingAudioFormat = new AudioFormat { Id = 1, Name = "7.1" };
            var newAudioFormat = new AudioFormat { Id = 2, Name = "Atmos" };

            GivenAudioProfile(upgradeForAudio: true, cutoffAudioFormatScore: 50, items: new List<ProfileAudioFormatItem>
            {
                new ProfileAudioFormatItem { Format = existingAudioFormat, Score = 10 },
                new ProfileAudioFormatItem { Format = newAudioFormat, Score = 20 }
            });

            Mocker.GetMock<IAudioFormatCalculationService>()
                  .Setup(s => s.ParseAudioFormats(episodeFile))
                  .Returns(new List<AudioFormat> { existingAudioFormat });

            _localEpisode.Quality = new QualityModel(Quality.Bluray720p);
            _localEpisode.AudioFormats = new List<AudioFormat> { existingAudioFormat, newAudioFormat };

            var result = Subject.IsSatisfiedBy(_localEpisode, null);
            result.Accepted.Should().BeFalse();
            result.Reason.Should().Be(ImportRejectionReason.NotQualityUpgrade);
        }

        private EpisodeFile GivenCustomFormatDowngradeScenario()
        {
            var episodeFileCustomFormats = Builder<CustomFormat>.CreateListOfSize(1).Build().ToList();

            var episodeFile = new EpisodeFile
            {
                Quality = new QualityModel(Quality.Bluray1080p),
                MediaInfo = new MediaInfoModel { AudioStreams = new List<MediaInfoAudioStreamModel>() }
            };

            _series.QualityProfile.Value.FormatItems = episodeFileCustomFormats.Select(c => new ProfileFormatItem
            {
                Format = c,
                Score = 50
            })
                .ToList();

            Mocker.GetMock<IConfigService>()
                .Setup(s => s.DownloadPropersAndRepacks)
                .Returns(ProperDownloadTypes.DoNotPrefer);

            Mocker.GetMock<ICustomFormatCalculationService>()
                .Setup(s => s.ParseCustomFormat(episodeFile))
                .Returns(episodeFileCustomFormats);

            _localEpisode.Quality = new QualityModel(Quality.Bluray1080p);
            _localEpisode.CustomFormats = Builder<CustomFormat>.CreateListOfSize(1).Build().ToList();
            _localEpisode.CustomFormatScore = 20;
            _localEpisode.MediaInfo = new MediaInfoModel { AudioStreams = new List<MediaInfoAudioStreamModel>() };

            _localEpisode.Episodes = Builder<Episode>.CreateListOfSize(1)
                .All()
                .With(e => e.EpisodeFileId = 1)
                .With(e => e.EpisodeFile = new LazyLoaded<EpisodeFile>(episodeFile))
                .Build()
                .ToList();

            return episodeFile;
        }

        private void GivenAudioProfile(bool upgradeForAudio, int cutoffAudioFormatScore, List<ProfileAudioFormatItem> items)
        {
            var profile = _series.QualityProfile.Value;
            profile.UpgradeForAudio = upgradeForAudio;
            profile.CutoffAudioFormatScore = cutoffAudioFormatScore;
            profile.AudioFormatItems = items;
        }
    }
}
