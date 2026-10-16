using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using NzbDrone.Core.AudioFormats;
using Sonarr.Http.ClientSchema;
using Sonarr.Http.REST;

namespace Sonarr.Api.V3.AudioFormats
{
    public class AudioFormatResource : RestResource
    {
        [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
        public override int Id { get; set; }
        public string Name { get; set; }
        public List<AudioFormatSpecificationSchema> Specifications { get; set; }
    }

    public static class AudioFormatResourceMapper
    {
        public static AudioFormatResource ToResource(this AudioFormat model, bool includeDetails)
        {
            var resource = new AudioFormatResource
            {
                Id = model.Id,
                Name = model.Name
            };

            if (includeDetails)
            {
                resource.Specifications = model.Specifications.Select(x => x.ToSchema()).ToList();
            }

            return resource;
        }

        public static List<AudioFormatResource> ToResource(this IEnumerable<AudioFormat> models, bool includeDetails)
        {
            return models.Select(m => m.ToResource(includeDetails)).ToList();
        }

        public static AudioFormat ToModel(this AudioFormatResource resource, List<IAudioFormatSpecification> specifications)
        {
            return new AudioFormat
            {
                Id = resource.Id,
                Name = resource.Name,
                Specifications = resource.Specifications?.Select(x => MapSpecification(x, specifications)).ToList() ?? new List<IAudioFormatSpecification>()
            };
        }

        private static IAudioFormatSpecification MapSpecification(AudioFormatSpecificationSchema resource, List<IAudioFormatSpecification> specifications)
        {
            var matchingSpec =
                specifications.SingleOrDefault(x => x.GetType().Name == resource.Implementation);

            if (matchingSpec is null)
            {
                throw new ArgumentException(
                    $"{resource.Implementation} is not a valid specification implementation");
            }

            var type = matchingSpec.GetType();

            // Finding the exact current specification isn't possible given the dynamic nature of them and the possibility that multiple
            // of the same type exist within the same format. Passing in null is safe as long as there never exists a specification that
            // relies on additional privacy.
            var spec = (IAudioFormatSpecification)SchemaBuilder.ReadFromSchema(resource.Fields, type, null);
            spec.Name = resource.Name;
            spec.Negate = resource.Negate;
            spec.Required = resource.Required;
            return spec;
        }
    }
}
