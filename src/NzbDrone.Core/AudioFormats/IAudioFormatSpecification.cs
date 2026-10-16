using NzbDrone.Core.MediaFiles.MediaInfo;
using NzbDrone.Core.Validation;

namespace NzbDrone.Core.AudioFormats
{
    public interface IAudioFormatSpecification
    {
        int Order { get; }
        string InfoLink { get; }
        string ImplementationName { get; }
        string Name { get; set; }
        bool Negate { get; set; }
        bool Required { get; set; }

        NzbDroneValidationResult Validate();

        IAudioFormatSpecification Clone();
        bool IsSatisfiedBy(AudioFormatInput input);

        // Per-stream evaluation used by the calculation service for group matching:
        // evaluate this specification against a single audio stream (negate applied).
        bool IsSatisfiedBy(AudioFormatInput input, MediaInfoAudioStreamModel stream);
    }
}
