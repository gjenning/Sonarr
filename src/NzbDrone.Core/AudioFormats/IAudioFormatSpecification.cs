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
    }
}
