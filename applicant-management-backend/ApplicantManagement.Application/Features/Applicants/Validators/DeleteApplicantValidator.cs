using ApplicantManagement.Application.Features.Applicants.Commands.DeleteApplicant;
using FluentValidation;

namespace ApplicantManagement.Application.Features.Applicants.Validators
{
    public class DeleteApplicantValidator : AbstractValidator<DeleteApplicantCommand>
    {
        public DeleteApplicantValidator()
        {
            RuleFor(x => x.Id)
                .GreaterThan(0)
                .WithMessage("Applicant ID must be greater than 0");

            RuleFor(x => x.RowVersion)
                .NotNull()
                .WithMessage("Row version is required for concurrency control")
                .Must(rowVersion => rowVersion != null && rowVersion.Length > 0)
                .WithMessage("Row version cannot be empty");

            RuleFor(x => x.Reason)
                .MaximumLength(500)
                .WithMessage("Reason must not exceed 500 characters")
                .When(x => !string.IsNullOrEmpty(x.Reason));

            RuleFor(x => x)
                .Must(x => !x.HardDelete || !string.IsNullOrWhiteSpace(x.Reason))
                .WithMessage("Reason is required for hard delete operations")
                .When(x => x.HardDelete);
        }
    }
}