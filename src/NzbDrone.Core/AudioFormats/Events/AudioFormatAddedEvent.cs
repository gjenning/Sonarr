using NzbDrone.Common.Messaging;

namespace NzbDrone.Core.AudioFormats.Events
{
    public class AudioFormatAddedEvent : IEvent
    {
        public AudioFormatAddedEvent(AudioFormat format)
        {
            AudioFormat = format;
        }

        public AudioFormat AudioFormat { get; private set; }
    }
}
