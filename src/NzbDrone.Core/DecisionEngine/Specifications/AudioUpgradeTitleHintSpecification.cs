using System.Linq;
using System.Text.RegularExpressions;
using NLog;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.AudioFormats;
using NzbDrone.Core.CustomFormats;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.Profiles.Qualities;

namespace NzbDrone.Core.DecisionEngine.Specifications
{
    public class AudioUpgradeTitleHintSpecification : IDownloadDecisionEngineSpecification
    {
        // v1 heuristic: a conservative list of audio keywords commonly found in release
        // titles. Extend the list here as needed; a future version could derive the
        // keywords from the audio format specification values instead.
        // Lookaround boundaries are used instead of \b so keywords ending in a
        // non-word character (e.g. "dd+") still match.
        private static readonly string[] AudioHintKeywords =
        {
            "truehd", "atmos", "dts-hd", "dts hd", "dts-x", "dts x", "dd+", "ddpa",
            "dd 5.1", "dd5.1", "eac3", "ac3", "pcm", "flac", "24bit", "24 bit", "7.1", "5.1"
        };

        private static readonly Regex AudioHintRegex = new(
            "(?<!\\w)(" + string.Join("|", AudioHintKeywords.Select(Regex.Escape)) + ")(?!\\w)",
            RegexOptions.IgnoreCase | RegexOptions.Compiled);

        private readonly UpgradableSpecification _upgradableSpecification;
        private readonly ICustomFormatCalculationService _formatService;
        private readonly IAudioFormatCalculationService _audioFormatService;
        private readonly Logger _logger;

        public AudioUpgradeTitleHintSpecification(UpgradableSpecification upgradableSpecification,
                                                  ICustomFormatCalculationService formatService,
                                                  IAudioFormatCalculationService audioFormatService,
                                                  Logger logger)
        {
            _upgradableSpecification = upgradableSpecification;
            _formatService = formatService;
            _audioFormatService = audioFormatService;
            _logger = logger;
        }

        public SpecificationPriority Priority => SpecificationPriority.Default;
        public RejectionType Type => RejectionType.Permanent;

        public virtual DownloadSpecDecision IsSatisfiedBy(RemoteEpisode subject, ReleaseDecisionInformation information)
        {
            var qualityProfile = subject.Series.QualityProfile.Value;

            if (!qualityProfile.UpgradeForAudio ||
                !qualityProfile.AudioUpgradeRequiresTitleHint ||
                !qualityProfile.AudioFormatItems.Any())
            {
                return DownloadSpecDecision.Accept();
            }

            if (!IsGrabAllowedOnlyForAudio(qualityProfile, subject))
            {
                return DownloadSpecDecision.Accept();
            }

            var title = subject.Release?.Title;

            if (title.IsNullOrWhiteSpace())
            {
                return DownloadSpecDecision.Accept();
            }

            if (AudioHintRegex.IsMatch(title))
            {
                _logger.Debug("Release title indicates an audio upgrade, keeping grab allowed for audio: {0}", title);
                return DownloadSpecDecision.Accept();
            }

            _logger.Debug("Release title does not indicate an audio upgrade, rejecting grab that was only allowed for audio: {0}", title);

            return DownloadSpecDecision.Reject(DownloadRejectionReason.AudioUpgradeTitleHintMissing, "Release title does not indicate an audio upgrade");
        }

        private bool IsGrabAllowedOnlyForAudio(QualityProfile qualityProfile, RemoteEpisode subject)
        {
            var newQuality = subject.ParsedEpisodeInfo.Quality;
            var newCustomFormats = subject.CustomFormats;

            foreach (var file in subject.Episodes.Where(c => c.EpisodeFileId != 0).Select(c => c.EpisodeFile.Value))
            {
                // Without MediaInfo the existing audio is unknown, so the audio bypass
                // never applies to this file.
                if (file == null || file.MediaInfo == null)
                {
                    continue;
                }

                var currentCustomFormats = _formatService.ParseCustomFormat(file);
                var currentAudioScore = qualityProfile.CalculateAudioFormatScore(_audioFormatService.ParseAudioFormats(file));

                // The audio bypass is the reason the grab is allowed when the upgrade
                // decision flips from rejected (audio unknown) to allowed (audio upgrade
                // pending), mirroring the bypass in UpgradableSpecification. Releases that
                // are genuine video or custom format upgrades never flip and are unaffected.
                var withoutAudio = _upgradableSpecification.IsUpgradable(qualityProfile, file.Quality, currentCustomFormats, newQuality, newCustomFormats);
                var withAudio = _upgradableSpecification.IsUpgradable(qualityProfile, file.Quality, currentCustomFormats, newQuality, newCustomFormats, currentAudioScore);

                if (withoutAudio != UpgradeableRejectReason.None && withAudio == UpgradeableRejectReason.None)
                {
                    return true;
                }
            }

            return false;
        }
    }
}
