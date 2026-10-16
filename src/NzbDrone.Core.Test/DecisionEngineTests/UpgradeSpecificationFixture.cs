using System.Collections.Generic;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.AudioFormats;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.CustomFormats;
using NzbDrone.Core.DecisionEngine;
using NzbDrone.Core.DecisionEngine.Specifications;
using NzbDrone.Core.Profiles;
using NzbDrone.Core.Profiles.Qualities;
using NzbDrone.Core.Qualities;
using NzbDrone.Core.Test.Framework;

namespace NzbDrone.Core.Test.DecisionEngineTests
{
    [TestFixture]

    public class UpgradeSpecificationFixture : CoreTest<UpgradableSpecification>
    {
        public static object[] IsUpgradeTestCases =
        {
            new object[] { Quality.SDTV, 1, Quality.SDTV, 2, Quality.SDTV, UpgradeableRejectReason.None },
            new object[] { Quality.WEBDL720p, 1, Quality.WEBDL720p, 2, Quality.WEBDL720p, UpgradeableRejectReason.None },
            new object[] { Quality.SDTV, 1, Quality.SDTV, 1, Quality.SDTV, UpgradeableRejectReason.CustomFormatScore },
            new object[] { Quality.WEBDL720p, 1, Quality.HDTV720p, 2, Quality.Bluray720p, UpgradeableRejectReason.BetterQuality },
            new object[] { Quality.WEBDL720p, 1, Quality.HDTV720p, 2, Quality.WEBDL720p, UpgradeableRejectReason.BetterQuality },
            new object[] { Quality.WEBDL720p, 1, Quality.WEBDL720p, 1, Quality.WEBDL720p, UpgradeableRejectReason.CustomFormatScore },
            new object[] { Quality.WEBDL1080p, 1, Quality.WEBDL1080p, 1, Quality.WEBDL1080p, UpgradeableRejectReason.CustomFormatScore }
        };

        private void GivenAutoDownloadPropers(ProperDownloadTypes type)
        {
            Mocker.GetMock<IConfigService>()
                  .SetupGet(s => s.DownloadPropersAndRepacks)
                  .Returns(type);
        }

        [Test]
        [TestCaseSource(nameof(IsUpgradeTestCases))]
        public void IsUpgradeTest(Quality current, int currentVersion, Quality newQuality, int newVersion, Quality cutoff, UpgradeableRejectReason expected)
        {
            GivenAutoDownloadPropers(ProperDownloadTypes.PreferAndUpgrade);

            var profile = new QualityProfile
            {
                UpgradeAllowed = true,
                Items = Qualities.QualityFixture.GetDefaultQualities()
            };

            Subject.IsUpgradable(
                       profile,
                       new QualityModel(current, new Revision(version: currentVersion)),
                       new List<CustomFormat>(),
                       new QualityModel(newQuality, new Revision(version: newVersion)),
                       new List<CustomFormat>())
                    .Should().Be(expected);
        }

        [Test]
        public void should_return_true_if_proper_and_download_propers_is_do_not_download()
        {
            GivenAutoDownloadPropers(ProperDownloadTypes.DoNotUpgrade);

            var profile = new QualityProfile
            {
                Items = Qualities.QualityFixture.GetDefaultQualities(),
            };

            Subject.IsUpgradable(
                       profile,
                       new QualityModel(Quality.DVD, new Revision(version: 1)),
                       new List<CustomFormat>(),
                       new QualityModel(Quality.DVD, new Revision(version: 2)),
                       new List<CustomFormat>())
                    .Should().Be(UpgradeableRejectReason.None);
        }

        [Test]
        public void should_return_false_if_proper_and_autoDownloadPropers_is_do_not_prefer()
        {
            GivenAutoDownloadPropers(ProperDownloadTypes.DoNotPrefer);

            var profile = new QualityProfile
            {
                Items = Qualities.QualityFixture.GetDefaultQualities(),
            };

            Subject.IsUpgradable(
                       profile,
                       new QualityModel(Quality.DVD, new Revision(version: 1)),
                       new List<CustomFormat>(),
                       new QualityModel(Quality.DVD, new Revision(version: 2)),
                       new List<CustomFormat>())
                   .Should().Be(UpgradeableRejectReason.UpgradesNotAllowed);
        }

        [Test]
        public void should_return_false_if_release_and_existing_file_are_the_same()
        {
            var profile = new QualityProfile
            {
                Items = Qualities.QualityFixture.GetDefaultQualities()
            };

            Subject.IsUpgradable(
                       profile,
                       new QualityModel(Quality.HDTV720p, new Revision(version: 1)),
                       new List<CustomFormat>(),
                       new QualityModel(Quality.HDTV720p, new Revision(version: 1)),
                       new List<CustomFormat>())
                   .Should().Be(UpgradeableRejectReason.UpgradesNotAllowed);
        }

        [Test]
        public void should_return_true_if_release_has_higher_quality_and_cutoff_is_not_already_met()
        {
            var profile = new QualityProfile
            {
                Items = Qualities.QualityFixture.GetDefaultQualities(),
                UpgradeAllowed = true,
                Cutoff = Quality.HDTV1080p.Id
            };

            Subject.IsUpgradable(
                    profile,
                    new QualityModel(Quality.HDTV720p, new Revision(version: 1)),
                    new List<CustomFormat>(),
                    new QualityModel(Quality.HDTV1080p, new Revision(version: 1)),
                    new List<CustomFormat>())
                .Should().Be(UpgradeableRejectReason.None);
        }

        [Test]
        public void should_return_false_if_release_has_higher_quality_and_cutoff_is_already_met()
        {
            var profile = new QualityProfile
            {
                Items = Qualities.QualityFixture.GetDefaultQualities(),
                UpgradeAllowed = true,
                Cutoff = Quality.HDTV720p.Id
            };

            Subject.IsUpgradable(
                    profile,
                    new QualityModel(Quality.HDTV720p, new Revision(version: 1)),
                    new List<CustomFormat>(),
                    new QualityModel(Quality.HDTV1080p, new Revision(version: 1)),
                    new List<CustomFormat>())
                .Should().Be(UpgradeableRejectReason.QualityCutoff);
        }

        [Test]
        public void should_return_false_if_minimum_custom_score_is_not_met()
        {
            var customFormatOne = new CustomFormat
            {
                Id = 1,
                Name = "One"
            };

            var customFormatTwo = new CustomFormat
            {
                Id = 2,
                Name = "Two"
            };

            var profile = new QualityProfile
            {
                Items = Qualities.QualityFixture.GetDefaultQualities(),
                UpgradeAllowed = true,
                MinUpgradeFormatScore = 11,
                CutoffFormatScore = 100,
                FormatItems = new List<ProfileFormatItem>
                {
                    new ProfileFormatItem
                    {
                        Format = customFormatOne,
                        Score = 10
                    },
                    new ProfileFormatItem
                    {
                        Format = customFormatTwo,
                        Score = 20
                    }
                }
            };

            Subject.IsUpgradable(
                    profile,
                    new QualityModel(Quality.DVD),
                    new List<CustomFormat> { customFormatOne },
                    new QualityModel(Quality.DVD),
                    new List<CustomFormat> { customFormatTwo })
                .Should().Be(UpgradeableRejectReason.MinCustomFormatScore);
        }

        [Test]
        public void should_return_true_if_minimum_custom_score_is_met()
        {
            var customFormatOne = new CustomFormat
            {
                Id = 1,
                Name = "One"
            };

            var customFormatTwo = new CustomFormat
            {
                Id = 2,
                Name = "Two"
            };

            var profile = new QualityProfile
            {
                Items = Qualities.QualityFixture.GetDefaultQualities(),
                UpgradeAllowed = true,
                MinUpgradeFormatScore = 10,
                CutoffFormatScore = 100,
                FormatItems = new List<ProfileFormatItem>
                {
                    new ProfileFormatItem
                    {
                        Format = customFormatOne,
                        Score = 10
                    },
                    new ProfileFormatItem
                    {
                        Format = customFormatTwo,
                        Score = 20
                    }
                }
            };

            Subject.IsUpgradable(
                    profile,
                    new QualityModel(Quality.DVD),
                    new List<CustomFormat> { customFormatOne },
                    new QualityModel(Quality.DVD),
                    new List<CustomFormat> { customFormatTwo })
                .Should().Be(UpgradeableRejectReason.None);
        }

        private QualityProfile GivenAudioProfile(bool upgradeForAudio, int cutoffAudioFormatScore)
        {
            return new QualityProfile
            {
                Cutoff = Quality.HDTV720p.Id,
                UpgradeAllowed = true,
                UpgradeForAudio = upgradeForAudio,
                CutoffAudioFormatScore = cutoffAudioFormatScore,
                AudioFormatItems = new List<ProfileAudioFormatItem>
                {
                    new ProfileAudioFormatItem
                    {
                        Format = new AudioFormat("DTS-HD") { Id = 1 },
                        Score = 10
                    }
                },
                Items = Qualities.QualityFixture.GetDefaultQualities()
            };
        }

        [Test]
        public void should_allow_same_quality_when_audio_upgrade_is_pending()
        {
            var hdtv720p = new QualityModel(Quality.HDTV720p);

            var profile = GivenAudioProfile(true, 10);

            Subject.IsUpgradable(
                    profile,
                    hdtv720p,
                    new List<CustomFormat>(),
                    hdtv720p,
                    new List<CustomFormat>(),
                    currentAudioScore: 0)
                .Should().Be(UpgradeableRejectReason.None);
        }

        [Test]
        public void should_reject_same_quality_when_upgrade_for_audio_is_disabled()
        {
            var hdtv720p = new QualityModel(Quality.HDTV720p);

            var profile = GivenAudioProfile(false, 10);

            Subject.IsUpgradable(
                    profile,
                    hdtv720p,
                    new List<CustomFormat>(),
                    hdtv720p,
                    new List<CustomFormat>(),
                    currentAudioScore: 0)
                .Should().Be(UpgradeableRejectReason.CustomFormatScore);
        }

        [Test]
        public void should_reject_same_quality_when_audio_cutoff_is_already_met()
        {
            var hdtv720p = new QualityModel(Quality.HDTV720p);

            var profile = GivenAudioProfile(true, 10);

            Subject.IsUpgradable(
                    profile,
                    hdtv720p,
                    new List<CustomFormat>(),
                    hdtv720p,
                    new List<CustomFormat>(),
                    currentAudioScore: 10)
                .Should().Be(UpgradeableRejectReason.CustomFormatScore);
        }

        [Test]
        public void should_reject_same_quality_when_current_audio_score_is_unknown()
        {
            var hdtv720p = new QualityModel(Quality.HDTV720p);

            var profile = GivenAudioProfile(true, 10);

            Subject.IsUpgradable(
                    profile,
                    hdtv720p,
                    new List<CustomFormat>(),
                    hdtv720p,
                    new List<CustomFormat>())
                .Should().Be(UpgradeableRejectReason.CustomFormatScore);
        }

        [Test]
        public void should_not_allow_pending_audio_upgrade_to_bypass_quality_downgrade()
        {
            var profile = GivenAudioProfile(true, 10);

            Subject.IsUpgradable(
                    profile,
                    new QualityModel(Quality.HDTV720p),
                    new List<CustomFormat>(),
                    new QualityModel(Quality.SDTV),
                    new List<CustomFormat>(),
                    currentAudioScore: 0)
                .Should().Be(UpgradeableRejectReason.BetterQuality);
        }
    }
}
