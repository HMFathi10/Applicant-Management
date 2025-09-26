using ApplicantManagement.Infrastructure.Services;
using FluentValidation;

namespace ApplicantManagement.Application.Features.Applicants.Validators
{
    public class CountryValidator : AbstractValidator<string>
    {
        private readonly ICountryValidationService _countryValidationService;

        public CountryValidator(ICountryValidationService countryValidationService)
        {
            _countryValidationService = countryValidationService;

            //RuleFor(country => country)
            //    .NotEmpty().WithMessage("Country is required")
            //    .Must(BeValidCountry).WithMessage("Country must be a valid country name");
        }

        private bool BeValidCountry(string country)
        {
            return _countryValidationService.ValidateCountry(country);
        }
    }
}