using ApplicantManagement.Domain.Entities;
using ApplicantManagement.Domain.Repositories;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace ApplicantManagement.Infrastructure.Repositories
{
    public interface IApplicantRepository : IRepository<Applicant>
    {
        Task<bool> EmailExistsAsync(string emailAddress, int? excludeId = null);
        Task<bool> HasActiveApplicationsAsync(string emailAddress);
        Task<IEnumerable<Applicant>> GetApplicantsWithFiltersAsync(
            string? searchTerm = null,
            int? minAge = null,
            int? maxAge = null,
            string? country = null,
            bool? hired = null,
            DateTime? appliedFrom = null,
            DateTime? appliedTo = null,
            string? sortBy = null,
            bool sortDescending = false,
            int page = 1,
            int pageSize = 10);
        Task<int> GetTotalCountAsync(Expression<Func<Applicant, bool>>? predicate = null);
        Task<Applicant?> GetByEmailAsync(string emailAddress);
        Task<bool> ValidateAgeAsync(int age);
        Task<bool> ValidateAppliedDateAsync(DateTime appliedDate);
        Task<IEnumerable<Applicant>> SearchApplicantsAsync(string searchTerm);
    }
}