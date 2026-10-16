using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using NzbDrone.Core.AudioFormats;
using NzbDrone.Integration.Test.Client;
using Sonarr.Api.V3.AudioFormats;
using Sonarr.Api.V3.Profiles.Quality;

namespace NzbDrone.Integration.Test.ApiTests
{
    [TestFixture]
    public class AudioFormatFixture : IntegrationTest
    {
        private ClientBase<AudioFormatResource> _audioFormats;

        [SetUp]
        public void AudioFormatSetUp()
        {
            _audioFormats = new ClientBase<AudioFormatResource>(RestClient, ApiKey);
        }

        private List<AudioFormatSpecificationSchema> GetSchema()
        {
            var request = _audioFormats.BuildRequest("schema");

            return _audioFormats.Get<List<AudioFormatSpecificationSchema>>(request);
        }

        // Builds a valid audio format from the server schema the same way the frontend does:
        // a single AudioStreamSpecification matching the codec regex "truehd".
        private AudioFormatResource GetAudioFormat(string name)
        {
            var specification = GetSchema().First();

            specification.Name = "Codec";
            specification.Fields.First(f => f.Name == "value").Value = "truehd";

            return new AudioFormatResource
            {
                Name = name,
                Specifications = new List<AudioFormatSpecificationSchema> { specification }
            };
        }

        [Test]
        public void schema_should_contain_audio_stream_field_options()
        {
            var schema = GetSchema();

            schema.Should().NotBeEmpty();
            schema.Should().OnlyContain(s => s.Implementation == nameof(AudioStreamSpecification));

            var field = schema.SelectMany(s => s.Fields).SingleOrDefault(f => f.Name == "field");

            field.Should().NotBeNull();
            field.Type.Should().Be("select");
            field.SelectOptions.Should().NotBeEmpty();
            field.SelectOptions.Select(o => o.Name).Should().BeEquivalentTo(
                "Codec", "Channels", "Bitrate", "Title", "Language", "AudioDescription", "StreamCount");
        }

        [Test]
        public void should_add_and_delete_audio_format()
        {
            var created = _audioFormats.Post(GetAudioFormat("TrueHD Test"));

            created.Id.Should().NotBe(0);

            _audioFormats.All().Should().Contain(c => c.Id == created.Id);

            created.Specifications.Should().HaveCount(1);
            created.Specifications.First().Implementation.Should().Be(nameof(AudioStreamSpecification));
            created.Specifications.First().Fields.First(f => f.Name == "value").Value.ToString().Should().Be("truehd");

            created.Name = "TrueHD Renamed";
            _audioFormats.Put(created);

            _audioFormats.Get(created.Id).Name.Should().Be("TrueHD Renamed");

            _audioFormats.Delete(created.Id);

            // GET by id for a deleted format throws KeyNotFoundException (500) just like the
            // Custom Formats endpoint, so absence is asserted through the list instead.
            _audioFormats.All().Should().NotContain(c => c.Id == created.Id);
        }

        [Test]
        public void should_not_allow_duplicate_names()
        {
            _audioFormats.Post(GetAudioFormat("Duplicate Format"));

            var response = _audioFormats.InvalidPost(GetAudioFormat("Duplicate Format"));

            var errors = JArray.FromObject(response);
            errors.Select(e => (string)e["errorMessage"]).Should().Contain("Must be unique.");
        }

        [Test]
        public void should_round_trip_audio_format_profile_fields()
        {
            var audioFormat = _audioFormats.Post(GetAudioFormat("Profile Format"));

            var profile = QualityProfiles.All().First();

            profile.AudioFormatItems = new List<ProfileAudioFormatItemResource>
            {
                new ProfileAudioFormatItemResource
                {
                    Format = audioFormat.Id,
                    Name = audioFormat.Name,
                    Score = 100
                }
            };
            profile.MinAudioFormatScore = 50;
            profile.CutoffAudioFormatScore = 100;
            profile.UpgradeForAudio = true;

            QualityProfiles.Put(profile);

            var updated = QualityProfiles.Get(profile.Id);

            updated.MinAudioFormatScore.Should().Be(50);
            updated.CutoffAudioFormatScore.Should().Be(100);
            updated.UpgradeForAudio.Should().BeTrue();
            updated.AudioFormatItems.Should().ContainSingle(i => i.Format == audioFormat.Id && i.Score == 100);
        }
    }
}
