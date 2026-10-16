using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using NzbDrone.Core.AudioFormats;

namespace NzbDrone.Core.Datastore.Converters
{
    public class AudioFormatIntConverter : JsonConverter<AudioFormat>
    {
        public override AudioFormat Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            return new AudioFormat { Id = reader.GetInt32() };
        }

        public override void Write(Utf8JsonWriter writer, AudioFormat value, JsonSerializerOptions options)
        {
            writer.WriteNumberValue(value.Id);
        }
    }
}
