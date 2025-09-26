using ApplicantManagement.Applicants.Services;
using ApplicantManagement.Application.DTOs;
using ApplicantManagement.Domain.Entities;
using ApplicantManagement.Domain.Repositories;
using ApplicantManagement.Infrastructure.Repositories;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ApplicantManagement.Application.Features.Applicants.Queries.GetApplicantsWithFilters
{
    public class GetApplicantsWithFiltersHandler : IRequestHandler<GetApplicantsWithFiltersQuery, PaginatedResponseDto<Applicant>>
    {
        private readonly IApplicantRepository _applicantRepository;
        private readonly ILogger<GetApplicantsWithFiltersHandler> _logger;
        private readonly IApplicantLoggingService _loggingService;
        private readonly IApplicantSecurityService _securityService;

        public GetApplicantsWithFiltersHandler(
            IApplicantRepository applicantRepository, 
            ILogger<GetApplicantsWithFiltersHandler> logger,
            IApplicantLoggingService loggingService,
            IApplicantSecurityService securityService)
        {
            _applicantRepository = applicantRepository ?? throw new ArgumentNullException(nameof(applicantRepository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _loggingService = loggingService ?? throw new ArgumentNullException(nameof(loggingService));
            _securityService = securityService ?? throw new ArgumentNullException(nameof(securityService));
        }

        public async Task<PaginatedResponseDto<Applicant>> Handle(GetApplicantsWithFiltersQuery request, CancellationToken cancellationToken)
        {
            if (request == null)
            {
                throw new ArgumentNullException(nameof(request), "Query request cannot be null");
            }

            try
            {
                _logger.LogInformation("Starting filtered query with parameters: SearchTerm={SearchTerm}, MinAge={MinAge}, MaxAge={MaxAge}, Country={Country}, IsHired={IsHired}, DateFrom={DateFrom}, DateTo={DateTo}, SortBy={SortBy}, Descending={Descending}, Page={Page}, PageSize={PageSize}, IncludeDeleted={IncludeDeleted}",
                    request.SearchTerm, request.MinAge, request.MaxAge, request.CountryOfOrigin, request.IsHired, 
                    request.AppliedDateFrom, request.AppliedDateTo, request.SortBy, request.SortDescending, 
                    request.PageNumber, request.PageSize, request.IncludeDeleted);

                // Validate input security
                ValidateInputSecurity(request);

                // Get total count before pagination
                var totalCount = await GetTotalCountAsync(request);

                // Retrieve applicants with filters, sorting, and pagination
                var applicants = await _applicantRepository.GetApplicantsWithFiltersAsync(
                    searchTerm: request.SearchTerm,
                    minAge: request.MinAge,
                    maxAge: request.MaxAge,
                    country: request.CountryOfOrigin,
                    hired: request.IsHired,
                    appliedFrom: request.AppliedDateFrom,
                    appliedTo: request.AppliedDateTo,
                    sortBy: request.SortBy,
                    sortDescending: request.SortDescending,
                    page: request.PageNumber,
                    pageSize: request.PageSize
                );

                var applicantList = applicants.ToList();
                _logger.LogInformation("Query completed successfully. Returned: {ReturnedRecords} records out of {TotalRecords} total",
                    applicantList.Count, totalCount);

                _loggingService.LogApplicantRetrieved(0, $"Filtered query returned {applicantList.Count} records out of {totalCount} total");

                // Return paginated response
                return new PaginatedResponseDto<Applicant>
                {
                    Items = applicantList,
                    TotalCount = totalCount,
                    CurrentPage = request.PageNumber,
                    PageSize = request.PageSize,
                    TotalPages = (int)Math.Ceiling((double)totalCount / request.PageSize),
                    HasPreviousPage = request.PageNumber > 1,
                    HasNextPage = request.PageNumber < (int)Math.Ceiling((double)totalCount / request.PageSize)
                };
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Business logic validation failed for filtered query");
                _loggingService.LogBusinessLogicError("GET", 0, ex.Message);
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while processing filtered query");
                _loggingService.LogSystemError("GET", 0, ex);
                throw new InvalidOperationException($"Failed to retrieve applicants: {ex.Message}", ex);
            }
        }

        private IQueryable<Applicant> ApplyFilters(IQueryable<Applicant> applicants, GetApplicantsWithFiltersQuery request)
        {
            var query = applicants.Where(a => !a.IsDeleted);

            if (!string.IsNullOrEmpty(request.CountryOfOrigin))
                query = query.Where(a => a.CountryOfOrigin.Contains(request.CountryOfOrigin));

            if (request.MinAge.HasValue)
                query = query.Where(a => a.Age >= request.MinAge.Value);

            if (request.MaxAge.HasValue)
                query = query.Where(a => a.Age <= request.MaxAge.Value);

            if (!string.IsNullOrEmpty(request.SearchTerm))
                query = query.Where(a => a.Name.Contains(request.SearchTerm) || 
                                       a.FamilyName.Contains(request.SearchTerm) || 
                                       a.EmailAddress.Contains(request.SearchTerm));

            if (request.AppliedDateFrom.HasValue)
                query = query.Where(a => a.AppliedDate >= request.AppliedDateFrom.Value);

            if (request.AppliedDateTo.HasValue)
                query = query.Where(a => a.AppliedDate <= request.AppliedDateTo.Value);

            if (request.IsHired.HasValue)
                query = query.Where(a => a.Hired == request.IsHired.Value);

            return query;
        }



        private IQueryable<Applicant> ApplyPagination(IQueryable<Applicant> applicants, GetApplicantsWithFiltersQuery request)
        {
            return applicants
                .Skip((request.PageNumber - 1) * request.PageSize)
                .Take(request.PageSize);
        }

        private async Task<int> GetTotalCountAsync(GetApplicantsWithFiltersQuery request)
        {
            // Get total count of filtered applicants (without pagination)
            var totalCount = await _applicantRepository.GetApplicantsWithFiltersAsync(
                searchTerm: request.SearchTerm,
                minAge: request.MinAge,
                maxAge: request.MaxAge,
                country: request.CountryOfOrigin,
                hired: request.IsHired,
                appliedFrom: request.AppliedDateFrom,
                appliedTo: request.AppliedDateTo,
                sortBy: null, // No sorting needed for count
                sortDescending: false,
                page: 1, // Get all results for count
                pageSize: int.MaxValue // Get all results for count
            );

            return totalCount.Count();
        }

        private void ValidateInputSecurity(GetApplicantsWithFiltersQuery request)
        {
            // Validate pagination parameters
            if (request.PageNumber < 1 || request.PageNumber > 10000)
            {
                var errorMessage = $"Invalid page number: {request.PageNumber}. Must be between 1 and 10000";
                _loggingService.LogSecurityError("GET", 0, errorMessage);
                throw new InvalidOperationException(errorMessage);
            }

            if (request.PageSize < 1 || request.PageSize > 100)
            {
                var errorMessage = $"Invalid page size: {request.PageSize}. Must be between 1 and 100";
                _loggingService.LogSecurityError("GET", 0, errorMessage);
                throw new InvalidOperationException(errorMessage);
            }

            // Validate search term
            if (!string.IsNullOrEmpty(request.SearchTerm))
            {
                if (request.SearchTerm.Length > 100)
                {
                    var errorMessage = "Search term exceeds maximum length of 100 characters";
                    _loggingService.LogSecurityError("GET", 0, errorMessage);
                    throw new InvalidOperationException(errorMessage);
                }

                // Check for SQL injection patterns
                if (_securityService.IsSqlInjectionAttempt(request.SearchTerm))
                {
                    var errorMessage = "Potential SQL injection detected in search term";
                    _loggingService.LogSecurityError("GET", 0, errorMessage);
                    throw new InvalidOperationException(errorMessage);
                }

                // Check for XSS patterns
                if (_securityService.IsXssAttempt(request.SearchTerm))
                {
                    var errorMessage = "Potential XSS attack detected in search term";
                    _loggingService.LogSecurityError("GET", 0, errorMessage);
                    throw new InvalidOperationException(errorMessage);
                }
            }

            // Validate country of origin
            if (!string.IsNullOrEmpty(request.CountryOfOrigin))
            {
                if (request.CountryOfOrigin.Length > 50)
                {
                    var errorMessage = "Country of origin exceeds maximum length of 50 characters";
                    _loggingService.LogSecurityError("GET", 0, errorMessage);
                    throw new InvalidOperationException(errorMessage);
                }

                // Sanitize country name
                request.CountryOfOrigin = _securityService.SanitizeInput(request.CountryOfOrigin);
            }

            // Validate age range
            if (request.MinAge.HasValue && request.MaxAge.HasValue)
            {
                if (request.MinAge > request.MaxAge)
                {
                    var errorMessage = "Minimum age cannot be greater than maximum age";
                    _loggingService.LogBusinessLogicError("GET", 0, errorMessage);
                    throw new InvalidOperationException(errorMessage);
                }

                if (request.MinAge < 18 || request.MaxAge > 100)
                {
                    var errorMessage = "Age range must be between 18 and 100";
                    _loggingService.LogBusinessLogicError("GET", 0, errorMessage);
                    throw new InvalidOperationException(errorMessage);
                }
            }

            // Validate date range
            if (request.AppliedDateFrom.HasValue && request.AppliedDateTo.HasValue)
            {
                if (request.AppliedDateFrom > request.AppliedDateTo)
                {
                    var errorMessage = "Start date cannot be after end date";
                    _loggingService.LogBusinessLogicError("GET", 0, errorMessage);
                    throw new InvalidOperationException(errorMessage);
                }

                if (request.AppliedDateFrom > DateTime.UtcNow)
                {
                    var errorMessage = "Start date cannot be in the future";
                    _loggingService.LogBusinessLogicError("GET", 0, errorMessage);
                    throw new InvalidOperationException(errorMessage);
                }
            }

            // Validate sort by parameter
            if (!string.IsNullOrEmpty(request.SortBy))
            {
                var allowedSortFields = new[] { "name", "familyname", "age", "applieddate", "countryoforigin", "email", "id" };
                if (!allowedSortFields.Contains(request.SortBy.ToLower()))
                {
                    var errorMessage = $"Invalid sort field: {request.SortBy}";
                    _loggingService.LogSecurityError("GET", 0, errorMessage);
                    throw new InvalidOperationException(errorMessage);
                }
            }
        }
    }
}