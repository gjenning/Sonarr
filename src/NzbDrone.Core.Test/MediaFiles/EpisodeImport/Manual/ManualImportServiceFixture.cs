using System.Collections.Generic;
using System.Linq;
using FizzWare.NBuilder;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using NzbDrone.Common.Disk;
using NzbDrone.Core.AudioFormats;
using NzbDrone.Core.CustomFormats;
using NzbDrone.Core.Datastore;
using NzbDrone.Core.Download;
using NzbDrone.Core.MediaFiles;
using NzbDrone.Core.MediaFiles.EpisodeImport;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.Profiles;
using NzbDrone.Core.Profiles.Qualities;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Core.Tv;

namespace NzbDrone.Core.Test.MediaFiles.EpisodeImport.Manual
{
    [TestFixture]
    public class ManualImportServiceFixture : CoreTest<NzbDrone.Core.MediaFiles.EpisodeImport.Manual.ManualImportService>
    {
        private AudioFormat _audioFormat;
        private Series _series;

        [SetUp]
        public void Setup()
        {
            _audioFormat = new AudioFormat
            {
                Id = 1,
                Name = "TrueHD Atmos"
            };

            var profile = new QualityProfile
            {
                Name = "Test Profile",
                AudioFormatItems = new List<ProfileAudioFormatItem>
                {
                    new ProfileAudioFormatItem
                    {
                        Format = _audioFormat,
                        Score = 100
                    }
                }
            };

            _series = Builder<Series>.CreateNew()
                .With(s => s.Path, @"T:\Drama\Series")
                .With(s => s.QualityProfile, new LazyLoaded<QualityProfile>(profile))
                .Build();

            Mocker.GetMock<IDiskProvider>()
                .Setup(s => s.GetFileSize(It.IsAny<string>()))
                .Returns(1024L);

            Mocker.GetMock<ICustomFormatCalculationService>()
                .Setup(s => s.ParseCustomFormat(It.IsAny<EpisodeFile>(), It.IsAny<Series>()))
                .Returns(new List<CustomFormat>());

            Mocker.GetMock<ILocalEpisodeCustomFormatCalculationService>()
                .Setup(s => s.ParseEpisodeCustomFormats(It.IsAny<LocalEpisode>()))
                .Returns(new List<CustomFormat>());
        }

        [Test]
        public void should_populate_audio_formats_and_score_for_existing_episode_file()
        {
            var episodeFile = Builder<EpisodeFile>.CreateNew()
                .With(f => f.RelativePath, @"Season 1\Series S01E01.mkv")
                .Build();

            var episode = Builder<Episode>.CreateNew()
                .With(e => e.SeasonNumber, 1)
                .With(e => e.EpisodeFileId, episodeFile.Id)
                .Build();

            Mocker.GetMock<ISeriesService>()
                .Setup(s => s.GetSeries(_series.Id))
                .Returns(_series);

            Mocker.GetMock<IMediaFileService>()
                .Setup(s => s.GetFilesBySeries(_series.Id))
                .Returns(new List<EpisodeFile> { episodeFile });

            Mocker.GetMock<IEpisodeService>()
                .Setup(s => s.GetEpisodeBySeries(_series.Id))
                .Returns(new List<Episode> { episode });

            Mocker.GetMock<IAudioFormatCalculationService>()
                .Setup(s => s.ParseAudioFormats(episodeFile))
                .Returns(new List<AudioFormat> { _audioFormat });

            var item = Subject.GetMediaFiles(_series.Id, null).Single();

            item.AudioFormats.Should().ContainEquivalentOf(_audioFormat);
            item.AudioFormatScore.Should().Be(100);
        }

        [Test]
        public void should_populate_audio_formats_and_score_when_parsing_files()
        {
            var folder = @"T:\Downloads\Series.S01";
            var videoFile = @"T:\Downloads\Series.S01\Series.S01E01.mkv";

            var localEpisode = new LocalEpisode
            {
                Path = videoFile,
                Series = _series,
                Episodes = new List<Episode>
                {
                    Builder<Episode>.CreateNew()
                        .With(e => e.SeasonNumber, 1)
                        .Build()
                }
            };

            Mocker.GetMock<IDiskProvider>()
                .Setup(s => s.FolderExists(folder))
                .Returns(true);

            Mocker.GetMock<ISeriesService>()
                .Setup(s => s.GetSeries(_series.Id))
                .Returns(_series);

            Mocker.GetMock<IDiskScanService>()
                .Setup(s => s.GetVideoFiles(It.IsAny<string>(), It.IsAny<bool>()))
                .Returns(new[] { videoFile });

            Mocker.GetMock<IDiskScanService>()
                .Setup(s => s.FilterPaths(It.IsAny<string>(), It.IsAny<IEnumerable<string>>(), It.IsAny<bool>()))
                .Returns(new List<string> { videoFile });

            Mocker.GetMock<IMakeImportDecision>()
                .Setup(s => s.GetImportDecisions(
                    It.IsAny<List<string>>(),
                    It.IsAny<Series>(),
                    It.IsAny<DownloadClientItem>(),
                    It.IsAny<ParsedEpisodeInfo>(),
                    It.IsAny<ParsedEpisodeInfo>(),
                    It.IsAny<bool>(),
                    It.IsAny<bool>()))
                .Returns(new List<ImportDecision> { new ImportDecision(localEpisode) });

            Mocker.GetMock<IAudioFormatCalculationService>()
                .Setup(s => s.ParseAudioFormats(localEpisode))
                .Returns(new List<AudioFormat> { _audioFormat });

            var item = Subject.GetMediaFiles(folder, null, _series.Id, false).Single();

            item.AudioFormats.Should().ContainEquivalentOf(_audioFormat);
            item.AudioFormatScore.Should().Be(100);
        }
    }
}
