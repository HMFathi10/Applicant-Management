using ApplicantManagement.Application.Features.Applicants.Queries.GetApplicantsWithFilters;
using FluentValidation;

namespace ApplicantManagement.Application.Features.Applicants.Validators
{
    public class GetApplicantsWithFiltersValidator : AbstractValidator<GetApplicantsWithFiltersQuery>
    {
        public GetApplicantsWithFiltersValidator()
        {
            RuleFor(x => x.PageNumber)
                .GreaterThan(0)
                .WithMessage("Page number must be greater than 0");

            RuleFor(x => x.PageSize)
                .InclusiveBetween(1, 100)
                .WithMessage("Page size must be between 1 and 100");

            RuleFor(x => x.MinAge)
                .InclusiveBetween(18, 65)
                .WithMessage("Minimum age must be between 18 and 65")
                .When(x => x.MinAge.HasValue);

            RuleFor(x => x.MaxAge)
                .InclusiveBetween(18, 65)
                .WithMessage("Maximum age must be between 18 and 65")
                .When(x => x.MaxAge.HasValue)
                .GreaterThanOrEqualTo(x => x.MinAge)
                .WithMessage("Maximum age must be greater than or equal to minimum age")
                .When(x => x.MinAge.HasValue && x.MaxAge.HasValue);

            RuleFor(x => x.CountryOfOrigin)
                .MaximumLength(100)
                .WithMessage("Country name must not exceed 100 characters")
                .When(x => !string.IsNullOrEmpty(x.CountryOfOrigin));

            RuleFor(x => x.SearchTerm)
                .MaximumLength(100)
                .WithMessage("Search term must not exceed 100 characters")
                .When(x => !string.IsNullOrEmpty(x.SearchTerm));

            RuleFor(x => x.AppliedDateFrom)
                .LessThanOrEqualTo(DateTime.UtcNow.Date)
                .WithMessage("Applied date from cannot be in the future")
                .When(x => x.AppliedDateFrom.HasValue);

            RuleFor(x => x.AppliedDateTo)
                .LessThanOrEqualTo(DateTime.UtcNow.Date)
                .WithMessage("Applied date to cannot be in the future")
                .When(x => x.AppliedDateTo.HasValue)
                .GreaterThanOrEqualTo(x => x.AppliedDateFrom)
                .WithMessage("Applied date to must be greater than or equal to applied date from")
                .When(x => x.AppliedDateFrom.HasValue && x.AppliedDateTo.HasValue);

            RuleFor(x => x.SortBy)
                .Must(sortBy => new[] { "Name", "FamilyName", "Age", "CountryOfOrigin", "AppliedDate", "Hired" }.Contains(sortBy))
                .WithMessage("SortBy must be one of: Name, FamilyName, Age, CountryOfOrigin, AppliedDate, Hired")
                .When(x => !string.IsNullOrEmpty(x.SortBy));
        }
    }
}