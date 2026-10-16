using System.Collections.Generic;
using System.Linq;
using FizzWare.NBuilder;
using Moq;
using NUnit.Framework;
using NzbDrone.Core.AudioFormats;
using NzbDrone.Core.AudioFormats.Events;
using NzbDrone.Core.CustomFormats;
using NzbDrone.Core.ImportLists;
using NzbDrone.Core.Lifecycle;
using NzbDrone.Core.Profiles;
using NzbDrone.Core.Profiles.Qualities;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Core.Tv;

namespace NzbDrone.Core.Test.Profiles
{
    [TestFixture]
    public class QualityProfileServiceFixture : CoreTest<QualityProfileService>
    {
        [SetUp]
        public void Setup()
        {
            Mocker.GetMock<IImportListFactory>()
                  .Setup(s => s.All())
                  .Returns(new List<ImportListDefinition>());

            Mocker.GetMock<IAudioFormatService>()
                  .Setup(s => s.All())
                  .Returns(new List<AudioFormat>());
        }

        [Test]
        public void init_should_add_default_profiles()
        {
            Mocker.GetMock<ICustomFormatService>()
                  .Setup(s => s.All())
                  .Returns(new List<CustomFormat>());

            Subject.Handle(new ApplicationStartedEvent());

            Mocker.GetMock<IQualityProfileRepository>()
                .Verify(v => v.Insert(It.IsAny<QualityProfile>()), Times.Exactly(6));
        }

        [Test]

        // This confirms that new profiles are added only if no other profiles exists.
        // We don't want to keep adding them back if a user deleted them on purpose.
        public void Init_should_skip_if_any_profiles_already_exist()
        {
            Mocker.GetMock<IQualityProfileRepository>()
                  .Setup(s => s.All())
                  .Returns(Builder<QualityProfile>.CreateListOfSize(2).Build().ToList());

            Subject.Handle(new ApplicationStartedEvent());

            Mocker.GetMock<IQualityProfileRepository>()
                .Verify(v => v.Insert(It.IsAny<QualityProfile>()), Times.Never());
        }

        [Test]
        public void should_not_be_able_to_delete_profile_if_assigned_to_series()
        {
            var profile = Builder<QualityProfile>.CreateNew()
                                          .With(p => p.Id = 2)
                                          .Build();

            var seriesList = Builder<Series>.CreateListOfSize(3)
                                            .Random(1)
                                            .With(c => c.QualityProfileId = profile.Id)
                                            .Build().ToList();

            Mocker.GetMock<ISeriesService>().Setup(c => c.GetAllSeries()).Returns(seriesList);
            Mocker.GetMock<IQualityProfileRepository>().Setup(c => c.Get(profile.Id)).Returns(profile);

            Assert.Throws<QualityProfileInUseException>(() => Subject.Delete(profile.Id));

            Mocker.GetMock<IQualityProfileRepository>().Verify(c => c.Delete(It.IsAny<int>()), Times.Never());
        }

        [Test]
        public void should_delete_profile_if_not_assigned_to_series()
        {
            var seriesList = Builder<Series>.CreateListOfSize(3)
                                            .All()
                                            .With(c => c.QualityProfileId = 2)
                                            .Build().ToList();

            Mocker.GetMock<ISeriesService>().Setup(c => c.GetAllSeries()).Returns(seriesList);

            Subject.Delete(1);

            Mocker.GetMock<IQualityProfileRepository>().Verify(c => c.Delete(1), Times.Once());
        }

        [Test]
        public void should_not_be_able_to_delete_profile_if_assigned_to_import_list()
        {
            var profile = Builder<QualityProfile>.CreateNew()
                                                 .With(p => p.Id = 1)
                                                 .Build();

            var seriesList = Builder<Series>.CreateListOfSize(3)
                                            .All()
                                            .With(c => c.QualityProfileId = 2)
                                            .Build().ToList();

            var importLists = Builder<ImportListDefinition>.CreateListOfSize(3)
                                                           .Random(1)
                                                           .Build().ToList();

            Mocker.GetMock<IQualityProfileRepository>().Setup(c => c.Get(profile.Id)).Returns(profile);
            Mocker.GetMock<ISeriesService>().Setup(c => c.GetAllSeries()).Returns(seriesList);

            Mocker.GetMock<IImportListFactory>()
                  .Setup(s => s.All())
                  .Returns(importLists);

            Assert.Throws<QualityProfileInUseException>(() => Subject.Delete(1));

            Mocker.GetMock<IQualityProfileRepository>().Verify(c => c.Delete(It.IsAny<int>()), Times.Never());
        }

        [Test]
        public void new_profile_should_have_sane_audio_defaults()
        {
            var profile = new QualityProfile();

            Assert.That(profile.AudioFormatItems, Is.Not.Null.And.Empty);
            Assert.That(profile.MinAudioFormatScore, Is.EqualTo(0));
            Assert.That(profile.CutoffAudioFormatScore, Is.EqualTo(0));
            Assert.That(profile.UpgradeForAudio, Is.False);
            Assert.That(profile.AudioUpgradeRequiresTitleHint, Is.True);
        }

        [Test]
        public void get_default_profile_should_include_audio_format_items_with_zero_scores()
        {
            Mocker.GetMock<ICustomFormatService>()
                  .Setup(s => s.All())
                  .Returns(new List<CustomFormat>());

            Mocker.GetMock<IAudioFormatService>()
                  .Setup(s => s.All())
                  .Returns(new List<AudioFormat>
                  {
                      new AudioFormat { Id = 1, Name = "Atmos" },
                      new AudioFormat { Id = 2, Name = "TrueHD" }
                  });

            var profile = Subject.GetDefaultProfile("Test");

            Assert.That(profile.AudioFormatItems.Count, Is.EqualTo(2));
            Assert.That(profile.AudioFormatItems.All(i => i.Score == 0), Is.True);
            Assert.That(profile.MinAudioFormatScore, Is.EqualTo(0));
            Assert.That(profile.CutoffAudioFormatScore, Is.EqualTo(0));
            Assert.That(profile.UpgradeForAudio, Is.False);
            Assert.That(profile.AudioUpgradeRequiresTitleHint, Is.True);
        }

        [Test]
        public void audio_format_added_event_should_add_audio_format_item_to_all_profiles()
        {
            var profiles = Builder<QualityProfile>.CreateListOfSize(2)
                                                  .All()
                                                  .With(p => p.AudioFormatItems = new List<ProfileAudioFormatItem>())
                                                  .Build().ToList();

            Mocker.GetMock<IQualityProfileRepository>()
                  .Setup(s => s.All())
                  .Returns(profiles);

            Subject.Handle(new AudioFormatAddedEvent(new AudioFormat { Id = 5, Name = "DTS-HD" }));

            foreach (var profile in profiles)
            {
                Assert.That(profile.AudioFormatItems.Count, Is.EqualTo(1));
                Assert.That(profile.AudioFormatItems[0].Format.Id, Is.EqualTo(5));
                Assert.That(profile.AudioFormatItems[0].Score, Is.EqualTo(0));
            }

            Mocker.GetMock<IQualityProfileRepository>()
                  .Verify(v => v.Update(It.IsAny<QualityProfile>()), Times.Exactly(2));
        }

        [Test]
        public void audio_format_deleted_event_should_remove_audio_format_items_and_reset_scores_when_empty()
        {
            var profile = Builder<QualityProfile>.CreateNew()
                                                 .With(p => p.Id = 1)
                                                 .With(p => p.MinAudioFormatScore = 10)
                                                 .With(p => p.CutoffAudioFormatScore = 20)
                                                 .With(p => p.AudioFormatItems = new List<ProfileAudioFormatItem>
                                                 {
                                                     new ProfileAudioFormatItem { Format = new AudioFormat { Id = 5 }, Score = 10 }
                                                 })
                                                 .Build();

            Mocker.GetMock<IQualityProfileRepository>()
                  .Setup(s => s.All())
                  .Returns(new List<QualityProfile> { profile });

            Subject.Handle(new AudioFormatDeletedEvent(new AudioFormat { Id = 5 }));

            Assert.That(profile.AudioFormatItems, Is.Empty);
            Assert.That(profile.MinAudioFormatScore, Is.EqualTo(0));
            Assert.That(profile.CutoffAudioFormatScore, Is.EqualTo(0));

            Mocker.GetMock<IQualityProfileRepository>()
                  .Verify(v => v.Update(profile), Times.Once());
        }

        [Test]
        public void audio_format_deleted_event_should_keep_remaining_audio_format_items()
        {
            var profile = Builder<QualityProfile>.CreateNew()
                                                 .With(p => p.Id = 1)
                                                 .With(p => p.MinAudioFormatScore = 10)
                                                 .With(p => p.CutoffAudioFormatScore = 20)
                                                 .With(p => p.AudioFormatItems = new List<ProfileAudioFormatItem>
                                                 {
                                                     new ProfileAudioFormatItem { Format = new AudioFormat { Id = 5 }, Score = 10 },
                                                     new ProfileAudioFormatItem { Format = new AudioFormat { Id = 6 }, Score = 5 }
                                                 })
                                                 .Build();

            Mocker.GetMock<IQualityProfileRepository>()
                  .Setup(s => s.All())
                  .Returns(new List<QualityProfile> { profile });

            Subject.Handle(new AudioFormatDeletedEvent(new AudioFormat { Id = 5 }));

            Assert.That(profile.AudioFormatItems.Count, Is.EqualTo(1));
            Assert.That(profile.AudioFormatItems[0].Format.Id, Is.EqualTo(6));
            Assert.That(profile.MinAudioFormatScore, Is.EqualTo(10));
            Assert.That(profile.CutoffAudioFormatScore, Is.EqualTo(20));
        }
    }
}
