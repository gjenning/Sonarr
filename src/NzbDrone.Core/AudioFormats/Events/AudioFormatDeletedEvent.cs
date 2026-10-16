using NzbDrone.Common.Messaging;

namespace NzbDrone.Core.AudioFormats.Events
{
    public class AudioFormatDeletedEvent : IEvent
    {
        public AudioFormatDeletedEvent(AudioFormat format)
        {
            AudioFormat = format;
        }

        public AudioFormat AudioFormat { get; private set; }
    }
}
