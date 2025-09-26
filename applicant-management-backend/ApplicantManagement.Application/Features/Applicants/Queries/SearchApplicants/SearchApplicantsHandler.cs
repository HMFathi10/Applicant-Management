using ApplicantManagement.Applicants.Services;
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

namespace ApplicantManagement.Application.Features.Applicants.Queries.SearchApplicants
{
    public class SearchApplicantsHandler : IRequestHandler<SearchApplicantsQuery, List<Applicant>>
    {
        private readonly IApplicantRepository _applicantRepository;
        private readonly ILogger<SearchApplicantsHandler> _logger;
        private readonly IApplicantLoggingService _loggingService;
        private readonly IApplicantSecurityService _securityService;

        public SearchApplicantsHandler(
            IApplicantRepository applicantRepository,
            ILogger<SearchApplicantsHandler> logger,
            IApplicantLoggingService loggingService,
            IApplicantSecurityService securityService)
        {
            _applicantRepository = applicantRepository ?? throw new ArgumentNullException(nameof(applicantRepository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _loggingService = loggingService ?? throw new ArgumentNullException(nameof(loggingService));
            _securityService = securityService ?? throw new ArgumentNullException(nameof(securityService));
        }

        public async Task<List<Applicant>> Handle(SearchApplicantsQuery request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Searching applicants with search term: {SearchTerm}", request.SearchTerm);

            try
            {
                // Validate search term for security
                ValidateSearchTerm(request.SearchTerm);

                // If search term is empty or null, return all non-deleted applicants
                if (string.IsNullOrWhiteSpace(request.SearchTerm))
                {
                    var allApplicants = await _applicantRepository.GetAllAsync(a => !a.IsDeleted);
                    return allApplicants.ToList();
                }

                // Use the repository's search functionality
                var searchResults = await _applicantRepository.SearchApplicantsAsync(request.SearchTerm);
                
                _logger.LogInformation("Search completed successfully. Found {Count} applicants matching search term: {SearchTerm}", 
                    searchResults.Count(), request.SearchTerm);

                return searchResults.ToList();
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Validation failed for search term: {SearchTerm}", request.SearchTerm);
                _loggingService.LogSecurityError("SEARCH", 0, ex.Message);
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while searching applicants with search term: {SearchTerm}", request.SearchTerm);
                throw;
            }
        }

        private void ValidateSearchTerm(string searchTerm)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
            {
                return; // Empty search term is valid and returns all applicants
            }

            // Validate length
            if (searchTerm.Length > 100)
            {
                throw new InvalidOperationException("Search term exceeds maximum length of 100 characters");
            }

            // Check for SQL injection patterns
            if (_securityService.IsSqlInjectionAttempt(searchTerm))
            {
                throw new InvalidOperationException("Potentially malicious search term detected");
            }
        }
    }
}