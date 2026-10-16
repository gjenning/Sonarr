using System.Collections.Generic;
using System.Linq;
using NzbDrone.Core.AudioFormats;
using NzbDrone.Core.CustomFormats;
using NzbDrone.Core.Datastore;
using NzbDrone.Core.Messaging.Events;

namespace NzbDrone.Core.Profiles.Qualities
{
    public interface IQualityProfileRepository : IBasicRepository<QualityProfile>
    {
        bool Exists(int id);
    }

    public class QualityProfileRepository : BasicRepository<QualityProfile>, IQualityProfileRepository
    {
        private readonly ICustomFormatService _customFormatService;
        private readonly IAudioFormatService _audioFormatService;

        public QualityProfileRepository(IMainDatabase database,
                                        IEventAggregator eventAggregator,
                                        ICustomFormatService customFormatService,
                                        IAudioFormatService audioFormatService)
            : base(database, eventAggregator)
        {
            _customFormatService = customFormatService;
            _audioFormatService = audioFormatService;
        }

        protected override List<QualityProfile> Query(SqlBuilder builder)
        {
            var cfs = _customFormatService.All().ToDictionary(c => c.Id);
            var audioFormats = _audioFormatService.All().ToDictionary(a => a.Id);

            var profiles = base.Query(builder);

            // Do the conversions from Id to full CustomFormat object here instead of in
            // CustomFormatIntConverter to remove need to for a static property containing
            // all the custom formats
            foreach (var profile in profiles)
            {
                var formatItems = new List<ProfileFormatItem>();

                foreach (var formatItem in profile.FormatItems)
                {
                    // Skip any format that has been removed, but the profile wasn't updated properly
                    if (cfs.ContainsKey(formatItem.Format.Id))
                    {
                        formatItem.Format = cfs[formatItem.Format.Id];

                        formatItems.Add(formatItem);
                    }
                }

                profile.FormatItems = formatItems;

                var audioFormatItems = new List<ProfileAudioFormatItem>();

                foreach (var audioFormatItem in profile.AudioFormatItems ?? new List<ProfileAudioFormatItem>())
                {
                    // Skip any audio format that has been removed, but the profile wasn't updated properly
                    if (audioFormats.ContainsKey(audioFormatItem.Format.Id))
                    {
                        audioFormatItem.Format = audioFormats[audioFormatItem.Format.Id];

                        audioFormatItems.Add(audioFormatItem);
                    }
                }

                profile.AudioFormatItems = audioFormatItems;
            }

            return profiles;
        }

        public bool Exists(int id)
        {
            return Query(p => p.Id == id).Count == 1;
        }
    }
}
