using System.Collections.Generic;

namespace NzbDrone.Core.AudioFormats
{
    // Canonical values the AudioStream specification can match exactly, surfaced as dropdown
    // options through the /audioformat/schema endpoint. Codec names must stay in sync with
    // the outputs of MediaInfoFormatter.FormatAudioCodec; channel layouts are the common
    // values of FormatAudioChannels formatted with the "0.0" pattern.
    public static class AudioStreamValueOptions
    {
        public static readonly List<string> CodecOptions = new List<string>
        {
            "AAC",
            "HE-AAC",
            "xHE-AAC",
            "AC3",
            "EAC3",
            "EAC3 Atmos",
            "TrueHD",
            "TrueHD Atmos",
            "DTS",
            "DTS-HD HRA",
            "DTS-HD MA",
            "DTS-X",
            "DTS-ES",
            "DTS 96/24",
            "DTS Express",
            "MP2",
            "MP3",
            "FLAC",
            "Opus",
            "Vorbis",
            "PCM",
            "WMA"
        };

        public static readonly List<string> ChannelOptions = new List<string>
        {
            "1.0",
            "2.0",
            "2.1",
            "3.1",
            "4.1",
            "5.1",
            "6.1",
            "7.1"
        };

        // Keyed by AudioStreamField member name so the schema hands the UI a
        // field-name -> options map instead of duplicating the lists there.
        public static readonly Dictionary<string, List<string>> ByField = new Dictionary<string, List<string>>
        {
            { nameof(AudioStreamField.Codec), CodecOptions },
            { nameof(AudioStreamField.Channels), ChannelOptions }
        };
    }
}
