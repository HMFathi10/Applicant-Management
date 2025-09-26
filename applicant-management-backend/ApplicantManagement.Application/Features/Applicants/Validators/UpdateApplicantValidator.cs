using ApplicantManagement.Application.Features.Applicants.Commands.UpdateApplicant;
using FluentValidation;
using System.Text.RegularExpressions;

namespace ApplicantManagement.Application.Features.Applicants.Validators
{
    public class UpdateApplicantValidator : AbstractValidator<UpdateApplicantCommand>
    {
        public UpdateApplicantValidator()
        {
            RuleFor(x => x.Id)
                .GreaterThan(0)
                .WithMessage("Applicant ID must be greater than 0");

            RuleFor(x => x.Name)
                .NotEmpty()
                .WithMessage("Name is required")
                .Length(2, 100)
                .WithMessage("Name must be between 2 and 100 characters")
                .Matches(@"^[a-zA-Z\s\-\']+$")
                .WithMessage("Name can only contain letters, spaces, hyphens, and apostrophes");

            RuleFor(x => x.FamilyName)
                .NotEmpty()
                .WithMessage("Family name is required")
                .Length(2, 100)
                .WithMessage("Family name must be between 2 and 100 characters")
                .Matches(@"^[a-zA-Z\s\-\']+$")
                .WithMessage("Family name can only contain letters, spaces, hyphens, and apostrophes");

            RuleFor(x => x.Address)
                .NotEmpty()
                .WithMessage("Address is required")
                .Length(10, 200)
                .WithMessage("Address must be between 10 and 200 characters");

            RuleFor(x => x.EmailAddress)
                .NotEmpty()
                .WithMessage("Email address is required")
                .EmailAddress()
                .WithMessage("Invalid email address format")
                .Length(5, 100)
                .WithMessage("Email address must be between 5 and 100 characters");

            RuleFor(x => x.Phone)
                .NotEmpty()
                .WithMessage("Phone number is required")
                .Matches(@"^\d{7,15}$")
                .WithMessage("Phone number must contain only digits and be between 7 and 15 characters long");

            RuleFor(x => x.Age)
                .InclusiveBetween(18, 65)
                .WithMessage("Age must be between 18 and 65 years");

            RuleFor(x => x.CountryOfOrigin)
                .NotEmpty()
                .WithMessage("Country of origin is required")
                .Length(2, 100)
                .WithMessage("Country name must be between 2 and 100 characters");

            RuleFor(x => x.AppliedDate)
                .NotEmpty()
                .WithMessage("Applied date is required")
                .LessThanOrEqualTo(DateTime.UtcNow.Date)
                .WithMessage("Applied date cannot be in the future");

            RuleFor(x => x.RowVersion)
                .NotNull()
                .WithMessage("Row version is required for concurrency control")
                .Must(rowVersion => rowVersion != null && rowVersion.Length > 0)
                .WithMessage("Row version cannot be empty");

            RuleFor(x => x)
                .Must(x => x.Name != x.FamilyName)
                .WithMessage("Name and family name cannot be identical");
        }
    }
}