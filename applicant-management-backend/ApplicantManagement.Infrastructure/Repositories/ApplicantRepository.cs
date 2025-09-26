using ApplicantManagement.Domain.Entities;
using ApplicantManagement.Domain.Repositories;
using ApplicantManagement.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace ApplicantManagement.Infrastructure.Repositories
{
    public class ApplicantRepository : Repository<Applicant>, IApplicantRepository
    {
        private readonly ILogger<ApplicantRepository> _logger;

        public ApplicantRepository(ApplicationDbContext context, ILogger<ApplicantRepository> logger) 
            : base(context)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<bool> EmailExistsAsync(string emailAddress, int? excludeId = null)
        {
            try
            {
                var query = _dbSet.Where(a => a.EmailAddress == emailAddress);
                
                if (excludeId.HasValue)
                {
                    query = query.Where(a => a.Id != excludeId.Value);
                }

                return await query.AnyAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking if email exists: {EmailAddress}", emailAddress);
                throw;
            }
        }

        public async Task<bool> HasActiveApplicationsAsync(string emailAddress)
        {
            try
            {
                return await _dbSet
                    .AnyAsync(a => a.EmailAddress == emailAddress && !a.Hired);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking active applications for email: {EmailAddress}", emailAddress);
                throw;
            }
        }

        public async Task<IEnumerable<Applicant>> GetApplicantsWithFiltersAsync(
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
            int pageSize = 10)
        {
            try
            {
                var query = _dbSet.Where(ap => ap.IsDeleted == false).AsQueryable();

                // Apply filters
                if (!string.IsNullOrWhiteSpace(searchTerm))
                {
                    var lowerSearchTerm = searchTerm.ToLower();
                    query = query.Where(a => 
                        a.Name.ToLower().Contains(lowerSearchTerm) ||
                        a.FamilyName.ToLower().Contains(lowerSearchTerm) ||
                        a.Address.ToLower().Contains(lowerSearchTerm) ||
                        a.CountryOfOrigin.ToLower().Contains(lowerSearchTerm) ||
                        a.Phone.ToLower().Contains(lowerSearchTerm) ||
                        a.EmailAddress.ToLower().Contains(lowerSearchTerm));
                }

                if (minAge.HasValue)
                    query = query.Where(a => a.Age >= minAge.Value);

                if (maxAge.HasValue)
                    query = query.Where(a => a.Age <= maxAge.Value);

                if (!string.IsNullOrWhiteSpace(country))
                    query = query.Where(a => a.CountryOfOrigin.ToLower() == country.ToLower());

                if (hired.HasValue)
                    query = query.Where(a => a.Hired == hired.Value);

                if (appliedFrom.HasValue)
                    query = query.Where(a => a.AppliedDate >= appliedFrom.Value);

                if (appliedTo.HasValue)
                    query = query.Where(a => a.AppliedDate <= appliedTo.Value);

                // Apply sorting
                query = sortBy?.ToLower() switch
                {
                    "name" => sortDescending ? query.OrderByDescending(a => a.Name) : query.OrderBy(a => a.Name),
                    "familyname" => sortDescending ? query.OrderByDescending(a => a.FamilyName) : query.OrderBy(a => a.FamilyName),
                    "email" => sortDescending ? query.OrderByDescending(a => a.EmailAddress) : query.OrderBy(a => a.EmailAddress),
                    "age" => sortDescending ? query.OrderByDescending(a => a.Age) : query.OrderBy(a => a.Age),
                    "applieddate" => sortDescending ? query.OrderByDescending(a => a.AppliedDate) : query.OrderBy(a => a.AppliedDate),
                    "country" => sortDescending ? query.OrderByDescending(a => a.CountryOfOrigin) : query.OrderBy(a => a.CountryOfOrigin),
                    _ => query.OrderBy(a => a.Id)
                };

                // Apply pagination
                var skip = (page - 1) * pageSize;
                query = query.Skip(skip).Take(pageSize);

                return await query.ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving applicants with filters");
                throw;
            }
        }

        public async Task<int> GetTotalCountAsync(Expression<Func<Applicant, bool>>? predicate = null)
        {
            try
            {
                var query = _dbSet.AsQueryable();
                
                if (predicate != null)
                {
                    query = query.Where(predicate);
                }

                return await query.CountAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting total applicant count");
                throw;
            }
        }

        public async Task<Applicant?> GetByEmailAsync(string emailAddress)
        {
            try
            {
                return await _dbSet
                    .FirstOrDefaultAsync(a => a.EmailAddress == emailAddress);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving applicant by email: {EmailAddress}", emailAddress);
                throw;
            }
        }

        public async Task<bool> ValidateAgeAsync(int age)
        {
            // Business rule: Age must be between 18 and 65
            return age >= 18 && age <= 65;
        }

        public async Task<bool> ValidateAppliedDateAsync(DateTime appliedDate)
        {
            // Business rule: Applied date cannot be in the future
            return appliedDate <= DateTime.UtcNow;
        }

        public async Task<IEnumerable<Applicant>> SearchApplicantsAsync(string searchTerm)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(searchTerm))
                {
                    return Enumerable.Empty<Applicant>();
                }

                // Search in multiple fields: Name, FamilyName, EmailAddress, CountryOfOrigin
                var normalizedSearchTerm = searchTerm.ToLower().Trim();
                
                var query = _dbSet
                    .Where(a => !a.IsDeleted && (
                        a.Name.ToLower().Contains(normalizedSearchTerm) ||
                        a.FamilyName.ToLower().Contains(normalizedSearchTerm) ||
                        a.EmailAddress.ToLower().Contains(normalizedSearchTerm) ||
                        a.CountryOfOrigin.ToLower().Contains(normalizedSearchTerm)
                    ))
                    .OrderBy(a => a.Name)
                    .ThenBy(a => a.FamilyName);

                return await query.ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching applicants with term: {SearchTerm}", searchTerm);
                throw;
            }
        }

        public new async Task AddAsync(Applicant entity)
        {
            // Data integrity checks before adding
            await ValidateApplicantDataAsync(entity, isUpdate: false);
            await base.AddAsync(entity);
        }

        public new async Task UpdateAsync(Applicant entity)
        {
            // Data integrity checks before updating
            await ValidateApplicantDataAsync(entity, isUpdate: true);
            await base.UpdateAsync(entity);
        }

        private async Task ValidateApplicantDataAsync(Applicant applicant, bool isUpdate)
        {
            if (applicant == null)
            {
                throw new ArgumentNullException(nameof(applicant), "Applicant cannot be null");
            }

            // Validate required fields
            if (string.IsNullOrWhiteSpace(applicant.Name))
            {
                throw new ArgumentException("Applicant name is required", nameof(applicant.Name));
            }

            if (string.IsNullOrWhiteSpace(applicant.FamilyName))
            {
                throw new ArgumentException("Applicant family name is required", nameof(applicant.FamilyName));
            }

            if (string.IsNullOrWhiteSpace(applicant.EmailAddress))
            {
                throw new ArgumentException("Applicant email address is required", nameof(applicant.EmailAddress));
            }

            // Validate email format
            if (!IsValidEmail(applicant.EmailAddress))
            {
                throw new ArgumentException("Invalid email address format", nameof(applicant.EmailAddress));
            }

            // Validate age
            if (!await ValidateAgeAsync(applicant.Age))
            {
                throw new ArgumentException("Applicant age must be between 18 and 65", nameof(applicant.Age));
            }

            // Validate applied date
            if (!await ValidateAppliedDateAsync(applicant.AppliedDate))
            {
                throw new ArgumentException("Applied date cannot be in the future", nameof(applicant.AppliedDate));
            }

            // Check for duplicate email (for create operations or when email is changed)
            if (!isUpdate || await EmailChangedAsync(applicant.Id, applicant.EmailAddress))
            {
                if (await EmailExistsAsync(applicant.EmailAddress, applicant.Id))
                {
                    throw new ArgumentException($"An applicant with email '{applicant.EmailAddress}' already exists", nameof(applicant.EmailAddress));
                }
            }

            // Validate phone number if provided
            if (!string.IsNullOrWhiteSpace(applicant.Phone))
            {
                if (!IsValidPhoneNumber(applicant.Phone))
                {
                    throw new ArgumentException("Invalid phone number format", nameof(applicant.Phone));
                }
            }

            _logger.LogInformation("Applicant data validation passed for applicant: {Name} {FamilyName}", 
                applicant.Name, applicant.FamilyName);
        }

        private async Task<bool> EmailChangedAsync(int applicantId, string newEmail)
        {
            var existingApplicant = await GetByIdAsync(applicantId);
            return existingApplicant?.EmailAddress != newEmail;
        }

        private bool IsValidEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return false;

            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }

        private bool IsValidPhoneNumber(string phone)
        {
            if (string.IsNullOrWhiteSpace(phone))
                return true; // Phone is optional

            // Basic phone validation - can be enhanced based on requirements
            return System.Text.RegularExpressions.Regex.IsMatch(phone, @"^[\d\s\+\-\(\)]{7,20}$");
        }
    }
}