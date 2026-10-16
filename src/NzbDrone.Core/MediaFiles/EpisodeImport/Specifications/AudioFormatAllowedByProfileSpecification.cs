using NLog;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Download;
using NzbDrone.Core.Parser.Model;

namespace NzbDrone.Core.MediaFiles.EpisodeImport.Specifications
{
    public class AudioFormatAllowedByProfileSpecification : IImportDecisionEngineSpecification
    {
        private readonly Logger _logger;

        public AudioFormatAllowedByProfileSpecification(Logger logger)
        {
            _logger = logger;
        }

        public ImportSpecDecision IsSatisfiedBy(LocalEpisode localEpisode, DownloadClientItem downloadClientItem)
        {
            var qualityProfile = localEpisode.Series.QualityProfile.Value;
            var minScore = qualityProfile.MinAudioFormatScore;

            if (minScore <= 0)
            {
                return ImportSpecDecision.Accept();
            }

            // The audio score can only be judged when media info is available.
            if (localEpisode.MediaInfo == null)
            {
                _logger.Trace("No media info available for {0}, cannot check audio format score", localEpisode.Path);
                return ImportSpecDecision.Accept();
            }

            var audioScore = qualityProfile.CalculateAudioFormatScore(localEpisode.AudioFormats);

            if (audioScore < minScore)
            {
                _logger.Debug("Audio formats {0} have score {1} below Series profile minimum {2}. Skipping {3}",
                    localEpisode.AudioFormats.ConcatToString(),
                    audioScore,
                    minScore,
                    localEpisode.Path);

                return ImportSpecDecision.Reject(ImportRejectionReason.AudioFormatMinimumScore,
                    "Audio Formats {0} have score {1} below Series profile minimum {2}",
                    localEpisode.AudioFormats.ConcatToString(),
                    audioScore,
                    minScore);
            }

            _logger.Trace("Audio Format Score of {0} [{1}] above Series profile minimum {2}",
                audioScore,
                localEpisode.AudioFormats.ConcatToString(),
                minScore);

            return ImportSpecDecision.Accept();
        }
    }
}
