using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.AudioFormats;
using NzbDrone.Core.Validation;
using Sonarr.Http;
using Sonarr.Http.REST;
using Sonarr.Http.REST.Attributes;

namespace Sonarr.Api.V5.AudioFormats;

[V5ApiController]
public class AudioFormatController : RestController<AudioFormatResource>
{
    private readonly IAudioFormatService _audioFormatService;
    private readonly List<IAudioFormatSpecification> _specifications;

    public AudioFormatController(IAudioFormatService audioFormatService,
                                 List<IAudioFormatSpecification> specifications)
    {
        _audioFormatService = audioFormatService;
        _specifications = specifications;

        SharedValidator.RuleFor(c => c.Name).NotEmpty();
        SharedValidator.RuleFor(c => c.Name)
            .Must((v, c) => !_audioFormatService.All().Any(f => f.Name == c && f.Id != v.Id)).WithMessage("Must be unique.");
        SharedValidator.RuleFor(c => c.Specifications).NotEmpty();
        SharedValidator.RuleFor(c => c).Custom((audioFormat, context) =>
        {
            if (audioFormat.Specifications is null || !audioFormat.Specifications.Any())
            {
                context.AddFailure("Must contain at least one Condition");
            }

            if (audioFormat.Specifications?.Any(s => s.Name.IsNullOrWhiteSpace()) == true)
            {
                context.AddFailure("Condition name(s) cannot be empty or consist of only spaces");
            }
        });
    }

    protected override AudioFormatResource GetResourceById(int id)
    {
        return _audioFormatService.GetById(id).ToResource(true);
    }

    [HttpGet]
    [Produces("application/json")]
    public Ok<List<AudioFormatResource>> GetAll()
    {
        return TypedResults.Ok(_audioFormatService.All().ToResource(true));
    }

    [RestPostById]
    [Consumes("application/json")]
    public Results<Created<AudioFormatResource>, NotFound> Create([FromBody] AudioFormatResource audioFormatResource)
    {
        var model = audioFormatResource.ToModel(_specifications);

        Validate(model);

        return TypedCreated(_audioFormatService.Insert(model).Id);
    }

    [RestPutById]
    [Consumes("application/json")]
    public Results<Accepted<AudioFormatResource>, NotFound> Update([FromBody] AudioFormatResource resource)
    {
        var model = resource.ToModel(_specifications);

        Validate(model);

        _audioFormatService.Update(model);

        return TypedAccepted(model.Id);
    }

    [RestDeleteById]
    public NoContent DeleteFormat(int id)
    {
        _audioFormatService.Delete(id);

        return TypedResults.NoContent();
    }

    [HttpGet("schema")]
    [Produces("application/json")]
    public Ok<List<AudioFormatSpecificationSchema>> GetTemplates()
    {
        // There is a single specification implementation (AudioStreamSpecification) whose
        // `field` select options (AudioStreamField enum) drive the UI's field selector, so
        // the schema is one entry carrying the enum options instead of one entry per
        // specification class like Custom Formats have.
        var schema = _specifications.OrderBy(x => x.Order).Select(x => x.ToSchema()).ToList();

        var presets = GetPresets().ToList();

        foreach (var item in schema)
        {
            item.Presets = presets.Where(x => x.GetType().Name == item.Implementation).Select(x => x.ToSchema()).ToList();
        }

        return TypedResults.Ok(schema);
    }

    private void Validate(AudioFormat definition)
    {
        foreach (var validationResult in definition.Specifications.Select(spec => spec.Validate()))
        {
            VerifyValidationResult(validationResult);
        }
    }

    private void VerifyValidationResult(ValidationResult validationResult)
    {
        var result = new NzbDroneValidationResult(validationResult.Errors);

        if (!result.IsValid)
        {
            throw new ValidationException(result.Errors);
        }
    }

    // Presets are the specifications of the existing audio formats, mirroring the Custom
    // Format controller; there are no hardcoded presets because every audio specification
    // targets stream metadata rather than release titles.
    private IEnumerable<IAudioFormatSpecification> GetPresets()
    {
        var formats = _audioFormatService.All();
        foreach (var format in formats)
        {
            foreach (var condition in format.Specifications)
            {
                var preset = condition.Clone();
                preset.Name = $"{format.Name}: {preset.Name}";
                yield return preset;
            }
        }
    }
}
