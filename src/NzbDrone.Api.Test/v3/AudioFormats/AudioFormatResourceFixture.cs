using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using NzbDrone.Core.AudioFormats;
using NzbDrone.Core.Localization;
using NzbDrone.Test.Common;
using Sonarr.Api.V3.AudioFormats;
using Sonarr.Http.ClientSchema;

namespace NzbDrone.Api.Test.v3.AudioFormats
{
    [TestFixture]
    public class AudioFormatResourceFixture : TestBase
    {
        private static readonly List<IAudioFormatSpecification> SpecificationTypes = new List<IAudioFormatSpecification>
        {
            new AudioStreamSpecification()
        };

        [SetUp]
        public void Setup()
        {
            Mocker.GetMock<ILocalizationService>()
                .Setup(s => s.GetLocalizedString(It.IsAny<string>(), It.IsAny<Dictionary<string, object>>()))
                .Returns<string, Dictionary<string, object>>((s, d) => s);

            SchemaBuilder.Initialize(Mocker.Container);
        }

        [Test]
        public void should_round_trip_model_through_resource()
        {
            var model = new AudioFormat
            {
                Id = 1,
                Name = "TrueHD Atmos",
                Specifications = new List<IAudioFormatSpecification>
                {
                    new AudioStreamSpecification
                    {
                        Name = "TrueHD",
                        Field = AudioStreamField.Codec,
                        Value = "TRUEHD"
                    },
                    new AudioStreamSpecification
                    {
                        Name = "Atmos",
                        Field = AudioStreamField.AudioDescription,
                        Value = "true"
                    }
                }
            };

            var resource = model.ToResource(true);
            var result = resource.ToModel(SpecificationTypes);

            result.Id.Should().Be(1);
            result.Name.Should().Be("TrueHD Atmos");
            result.Specifications.Count.Should().Be(2);

            var codec = (AudioStreamSpecification)result.Specifications.First(s => s.Name == "TrueHD");
            codec.Field.Should().Be(AudioStreamField.Codec);
            codec.Value.Should().Be("TRUEHD");

            var audioDescription = (AudioStreamSpecification)result.Specifications.First(s => s.Name == "Atmos");
            audioDescription.Field.Should().Be(AudioStreamField.AudioDescription);
            audioDescription.Value.Should().Be("true");
        }

        [Test]
        public void should_not_include_specifications_without_details()
        {
            var model = new AudioFormat
            {
                Id = 1,
                Name = "Test",
                Specifications = new List<IAudioFormatSpecification>
                {
                    new AudioStreamSpecification { Name = "Codec", Value = "AC3" }
                }
            };

            var resource = model.ToResource(false);

            resource.Specifications.Should().BeNull();
        }

        [Test]
        public void should_map_field_options_to_schema()
        {
            var schema = new AudioStreamSpecification().ToSchema();

            schema.Implementation.Should().Be("AudioStreamSpecification");

            var field = schema.Fields.Single(f => f.Name == "field");
            field.Type.Should().Be("select");
            field.SelectOptions.Select(o => o.Name).Should().Equal(
                "Codec", "Channels", "Bitrate", "Title", "Language", "AudioDescription", "StreamCount");
        }
    }
}
