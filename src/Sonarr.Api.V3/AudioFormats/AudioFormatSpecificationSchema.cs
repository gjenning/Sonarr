using System.Collections.Generic;
using NzbDrone.Core.AudioFormats;
using Sonarr.Http.ClientSchema;
using Sonarr.Http.REST;

namespace Sonarr.Api.V3.AudioFormats
{
    public class AudioFormatSpecificationSchema : RestResource
    {
        public string Name { get; set; }
        public string Implementation { get; set; }
        public string ImplementationName { get; set; }
        public string InfoLink { get; set; }
        public bool Negate { get; set; }
        public bool Required { get; set; }
        public List<Field> Fields { get; set; }
        public List<AudioFormatSpecificationSchema> Presets { get; set; }

        // Per-field dropdown options (field name -> allowed exact values) for fields whose
        // value is drawn from a fixed set, e.g. Codec and Channels. Values outside the list
        // are still accepted by the API and matched as regex.
        public Dictionary<string, List<string>> ValueOptions { get; set; }
    }

    public static class AudioFormatSpecificationSchemaMapper
    {
        public static AudioFormatSpecificationSchema ToSchema(this IAudioFormatSpecification model)
        {
            return new AudioFormatSpecificationSchema
            {
                Name = model.Name,
                Implementation = model.GetType().Name,
                ImplementationName = model.ImplementationName,
                InfoLink = model.InfoLink,
                Negate = model.Negate,
                Required = model.Required,
                Fields = SchemaBuilder.ToSchema(model),
                ValueOptions = AudioStreamValueOptions.ByField
            };
        }
    }
}
