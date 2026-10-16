using NzbDrone.Core.Datastore;
using NzbDrone.Core.Messaging.Events;

namespace NzbDrone.Core.AudioFormats
{
    public interface IAudioFormatRepository : IBasicRepository<AudioFormat>
    {
    }

    public class AudioFormatRepository : BasicRepository<AudioFormat>, IAudioFormatRepository
    {
        public AudioFormatRepository(IMainDatabase database, IEventAggregator eventAggregator)
            : base(database, eventAggregator)
        {
        }
    }
}
