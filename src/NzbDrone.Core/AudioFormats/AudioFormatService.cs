using System;
using System.Collections.Generic;
using System.Linq;
using FluentValidation;
using FluentValidation.Results;
using NzbDrone.Common.Cache;
using NzbDrone.Core.AudioFormats.Events;
using NzbDrone.Core.Messaging.Events;
using NzbDrone.Core.Validation;

namespace NzbDrone.Core.AudioFormats
{
    public interface IAudioFormatService
    {
        void Update(AudioFormat audioFormat);
        void Update(List<AudioFormat> audioFormat);
        AudioFormat Insert(AudioFormat audioFormat);
        List<AudioFormat> All();
        AudioFormat GetById(int id);
        void Delete(int id);
        void Delete(List<int> ids);
    }

    public class AudioFormatService : IAudioFormatService
    {
        private readonly IAudioFormatRepository _audioFormatRepository;
        private readonly IEventAggregator _eventAggregator;
        private readonly ICached<Dictionary<int, AudioFormat>> _cache;

        public AudioFormatService(IAudioFormatRepository audioFormatRepository,
                                  ICacheManager cacheManager,
                                  IEventAggregator eventAggregator)
        {
            _audioFormatRepository = audioFormatRepository;
            _eventAggregator = eventAggregator;
            _cache = cacheManager.GetCache<Dictionary<int, AudioFormat>>(typeof(AudioFormat), "formats");
        }

        private Dictionary<int, AudioFormat> AllDictionary()
        {
            return _cache.Get("all", () => _audioFormatRepository.All().ToDictionary(m => m.Id));
        }

        public List<AudioFormat> All()
        {
            return AllDictionary().Values.ToList();
        }

        public AudioFormat GetById(int id)
        {
            return AllDictionary()[id];
        }

        public void Update(AudioFormat audioFormat)
        {
            ValidateName(audioFormat);

            _audioFormatRepository.Update(audioFormat);
            _cache.Clear();
        }

        public void Update(List<AudioFormat> audioFormats)
        {
            audioFormats.ForEach(ValidateName);

            _audioFormatRepository.UpdateMany(audioFormats);
            _cache.Clear();
        }

        public AudioFormat Insert(AudioFormat audioFormat)
        {
            ValidateName(audioFormat);

            // Add to DB then insert into profiles
            var result = _audioFormatRepository.Insert(audioFormat);
            _cache.Clear();

            _eventAggregator.PublishEvent(new AudioFormatAddedEvent(result));

            return result;
        }

        public void Delete(int id)
        {
            var format = _audioFormatRepository.Get(id);

            // Remove from profiles before removing from DB
            _eventAggregator.PublishEvent(new AudioFormatDeletedEvent(format));

            _audioFormatRepository.Delete(id);
            _cache.Clear();
        }

        public void Delete(List<int> ids)
        {
            foreach (var id in ids)
            {
                var format = _audioFormatRepository.Get(id);

                // Remove from profiles before removing from DB
                _eventAggregator.PublishEvent(new AudioFormatDeletedEvent(format));

                _audioFormatRepository.Delete(id);
            }

            _cache.Clear();
        }

        private void ValidateName(AudioFormat audioFormat)
        {
            if (_audioFormatRepository.All().Any(f => f.Id != audioFormat.Id &&
                                                     string.Equals(f.Name, audioFormat.Name, StringComparison.OrdinalIgnoreCase)))
            {
                throw new ValidationException(new List<ValidationFailure>
                                              {
                                                  new NzbDroneValidationFailure("Name", "name already exists")
                                                  {
                                                      DetailedDescription = $"An audio format with name '{audioFormat.Name}' already exists."
                                                  }
                                              });
            }
        }
    }
}
