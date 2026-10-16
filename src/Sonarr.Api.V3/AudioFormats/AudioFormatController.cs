using System.Collections.Generic;
using System.Linq;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Mvc;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.AudioFormats;
using NzbDrone.Core.Validation;
using Sonarr.Http;
using Sonarr.Http.REST;
using Sonarr.Http.REST.Attributes;

namespace Sonarr.Api.V3.AudioFormats
{
    [V3ApiController]
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
                if (!audioFormat.Specifications.Any())
                {
                    context.AddFailure("Must contain at least one Condition");
                }

                if (audioFormat.Specifications.Any(s => s.Name.IsNullOrWhiteSpace()))
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
        public List<AudioFormatResource> GetAll()
        {
            return _audioFormatService.All().ToResource(true);
        }

        [RestPostById]
        [Consumes("application/json")]
        public ActionResult<AudioFormatResource> Create([FromBody] AudioFormatResource audioFormatResource)
        {
            var model = audioFormatResource.ToModel(_specifications);

            Validate(model);

            return Created(_audioFormatService.Insert(model).Id);
        }

        [RestPutById]
        [Consumes("application/json")]
        public ActionResult<AudioFormatResource> Update([FromBody] AudioFormatResource resource)
        {
            var model = resource.ToModel(_specifications);

            Validate(model);

            _audioFormatService.Update(model);

            return Accepted(model.Id);
        }

        [RestDeleteById]
        public void DeleteFormat(int id)
        {
            _audioFormatService.Delete(id);
        }

        [HttpGet("schema")]
        public object GetTemplates()
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

            return schema;
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
}
