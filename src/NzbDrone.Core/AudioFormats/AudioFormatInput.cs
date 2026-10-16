using NzbDrone.Core.MediaFiles.MediaInfo;

namespace NzbDrone.Core.AudioFormats
{
    public class AudioFormatInput
    {
        public MediaInfoModel MediaInfo { get; set; }
        public string SceneName { get; set; }
    }
}
