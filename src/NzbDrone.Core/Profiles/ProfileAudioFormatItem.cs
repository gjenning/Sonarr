using NzbDrone.Core.AudioFormats;
using NzbDrone.Core.Datastore;

namespace NzbDrone.Core.Profiles
{
    public class ProfileAudioFormatItem : IEmbeddedDocument
    {
        public AudioFormat Format { get; set; }
        public int Score { get; set; }
    }
}
