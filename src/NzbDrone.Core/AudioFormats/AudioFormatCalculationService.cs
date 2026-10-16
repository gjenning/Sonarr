using System.Collections.Generic;
using System.IO;
using System.Linq;
using NLog;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.MediaFiles;
using NzbDrone.Core.MediaFiles.MediaInfo;
using NzbDrone.Core.Parser.Model;

namespace NzbDrone.Core.AudioFormats
{
    public interface IAudioFormatCalculationService
    {
        List<AudioFormat> ParseAudioFormats(MediaInfoModel mediaInfo, string sceneName);
        List<AudioFormat> ParseAudioFormats(EpisodeFile episodeFile);
        List<AudioFormat> ParseAudioFormats(LocalEpisode localEpisode);
    }

    public class AudioFormatCalculationService : IAudioFormatCalculationService
    {
        private readonly IAudioFormatService _audioFormatService;
        private readonly Logger _logger;

        public AudioFormatCalculationService(IAudioFormatService audioFormatService, Logger logger)
        {
            _audioFormatService = audioFormatService;
            _logger = logger;
        }

        public List<AudioFormat> ParseAudioFormats(MediaInfoModel mediaInfo, string sceneName)
        {
            return ParseAudioFormats(
                new AudioFormatInput
                {
                    MediaInfo = mediaInfo,
                    SceneName = sceneName
                },
                _audioFormatService.All());
        }

        public List<AudioFormat> ParseAudioFormats(EpisodeFile episodeFile)
        {
            var releaseTitle = GetReleaseTitle(episodeFile);

            return ParseAudioFormats(
                new AudioFormatInput
                {
                    MediaInfo = episodeFile.MediaInfo,
                    SceneName = releaseTitle
                },
                _audioFormatService.All());
        }

        public List<AudioFormat> ParseAudioFormats(LocalEpisode localEpisode)
        {
            // LocalEpisode has no ReleaseTitle property; GetSceneOrFileName() is the
            // scene-name-with-filename-fallback the CF calculation service uses.
            return ParseAudioFormats(
                new AudioFormatInput
                {
                    MediaInfo = localEpisode.MediaInfo,
                    SceneName = localEpisode.GetSceneOrFileName()
                },
                _audioFormatService.All());
        }

        private string GetReleaseTitle(EpisodeFile episodeFile)
        {
            if (episodeFile.SceneName.IsNotNullOrWhiteSpace())
            {
                _logger.Trace("Using scene name for release title: {0}", episodeFile.SceneName);
                return episodeFile.SceneName;
            }

            if (episodeFile.OriginalFilePath.IsNotNullOrWhiteSpace())
            {
                _logger.Trace("Using original file path for release title: {0}", Path.GetFileName(episodeFile.OriginalFilePath));
                return Path.GetFileName(episodeFile.OriginalFilePath);
            }

            if (episodeFile.RelativePath.IsNotNullOrWhiteSpace())
            {
                _logger.Trace("Using relative path for release title: {0}", Path.GetFileName(episodeFile.RelativePath));
                return Path.GetFileName(episodeFile.RelativePath);
            }

            return string.Empty;
        }

        private static List<AudioFormat> ParseAudioFormats(AudioFormatInput input, List<AudioFormat> allAudioFormats)
        {
            var matches = new List<AudioFormat>();

            // No MediaInfo means the audio logic is silently off (locked decision).
            if (input?.MediaInfo == null)
            {
                return matches;
            }

            var streams = input.MediaInfo.AudioStreams ?? new List<MediaInfoAudioStreamModel>();

            foreach (var audioFormat in allAudioFormats)
            {
                // A format without specifications never matches (unlike Custom Formats,
                // where an empty specification list matches everything).
                if (audioFormat.Specifications == null || !audioFormat.Specifications.Any())
                {
                    continue;
                }

                var isMatch = audioFormat.Specifications
                    .GroupBy(t => t.GetType())
                    .All(g => GroupMatches(g, input, streams));

                if (isMatch)
                {
                    matches.Add(audioFormat);
                }
            }

            return matches.OrderBy(x => x.Name).ToList();
        }

        // Locked group semantics: the group matches when at least one audio stream
        // satisfies all specifications in the group (negate applies per specification
        // per stream, see AudioStreamSpecification).
        private static bool GroupMatches(IEnumerable<IAudioFormatSpecification> group, AudioFormatInput input, List<MediaInfoAudioStreamModel> streams)
        {
            // StreamCount specifications are evaluated on the stream set and ignore the
            // stream argument, so a file without audio streams still gets one evaluation
            // pass (with a null stream) to let e.g. "StreamCount = 0" match.
            if (streams.Count == 0)
            {
                return group.All(spec => spec.IsSatisfiedBy(input, null));
            }

            return streams.Any(stream => group.All(spec => spec.IsSatisfiedBy(input, stream)));
        }
    }
}
