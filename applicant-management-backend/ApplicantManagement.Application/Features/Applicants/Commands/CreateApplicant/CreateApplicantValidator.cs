using FluentValidation;
using ApplicantManagement.Infrastructure.Services;
using System.Threading;
using System.Threading.Tasks;

namespace ApplicantManagement.Application.Features.Applicants.Commands.CreateApplicant
{
    public class CreateApplicantValidator : AbstractValidator<CreateApplicantCommand>
    {
        private readonly ICountryValidationService _countryValidationService;

        public CreateApplicantValidator(ICountryValidationService countryValidationService)
        {
            _countryValidationService = countryValidationService;

            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Name is required")
                .MinimumLength(5).WithMessage("Name must be at least 5 characters")
                .MaximumLength(100).WithMessage("Name must not exceed 100 characters");

            RuleFor(x => x.FamilyName)
                .NotEmpty().WithMessage("Family Name is required")
                .MinimumLength(5).WithMessage("Family Name must be at least 5 characters")
                .MaximumLength(100).WithMessage("Family Name must not exceed 100 characters");

            RuleFor(x => x.Address)
                .NotEmpty().WithMessage("Address is required")
                .MinimumLength(10).WithMessage("Address must be at least 10 characters")
                .MaximumLength(255).WithMessage("Address must not exceed 255 characters");

            RuleFor(x => x.EmailAddress)
                .NotEmpty().WithMessage("Email Address is required")
                .Matches(@"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$").WithMessage("Invalid email format")
                .MaximumLength(100).WithMessage("Email must not exceed 100 characters");

            RuleFor(x => x.Phone)
                .NotEmpty().WithMessage("Phone is required")
                .Matches(@"^\+20\d{10}$").WithMessage("Phone number must be exactly 10 digits after +20 prefix");

            RuleFor(x => x.Age)
                .NotNull().WithMessage("Age is required")
                .InclusiveBetween(20, 60).WithMessage("Age must be between 20 and 60");

            RuleFor(x => x.CountryOfOrigin)
                .NotEmpty().WithMessage("Country of Origin is required")
                .Must(BeValidCountry).WithMessage("Country must be a valid country name");
        }

        private bool BeValidCountry(string country)
        {
            return _countryValidationService.ValidateCountry(country);
        }
    }
}