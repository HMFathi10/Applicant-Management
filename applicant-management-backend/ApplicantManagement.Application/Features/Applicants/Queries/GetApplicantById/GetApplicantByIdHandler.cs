using MediatR;
using ApplicantManagement.Domain.Entities;
using ApplicantManagement.Domain.Repositories;
using ApplicantManagement.Infrastructure.Repositories;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using ApplicantManagement.Applicants.Services;

namespace ApplicantManagement.Application.Features.Applicants.Queries.GetApplicantById
{
    public class GetApplicantByIdHandler : IRequestHandler<GetApplicantByIdQuery, Applicant?>
    {
        private readonly IApplicantRepository _applicantRepository;
        private readonly ILogger<GetApplicantByIdHandler> _logger;
        private readonly IApplicantLoggingService _loggingService;
        private readonly IApplicantSecurityService _securityService;

        public GetApplicantByIdHandler(
            IApplicantRepository applicantRepository,
            ILogger<GetApplicantByIdHandler> logger,
            IApplicantLoggingService loggingService,
            IApplicantSecurityService securityService)
        {
            _applicantRepository = applicantRepository;
            _logger = logger;
            _loggingService = loggingService;
            _securityService = securityService;
        }

        public async Task<Applicant?> Handle(GetApplicantByIdQuery request, CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("Retrieving applicant with ID: {ApplicantId}", request.Id);
                
                // Validate input security
                ValidateInputSecurity(request);

                var applicant = await _applicantRepository.GetByIdAsync(request.Id);
                
                if (applicant == null)
                {
                    _logger.LogWarning("Applicant with ID {ApplicantId} not found", request.Id);
                    _loggingService.LogBusinessLogicError("GET_BY_ID", request.Id, "Applicant not found");
                    return null;
                }

                _logger.LogInformation("Successfully retrieved applicant with ID: {ApplicantId}", request.Id);
                _loggingService.LogApplicantRetrieved(applicant.Id, $"{applicant.Name} {applicant.FamilyName}");
                
                return applicant;
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogError(ex, "Business logic error while retrieving applicant with ID: {ApplicantId}", request.Id);
                _loggingService.LogBusinessLogicError("GET_BY_ID", request.Id, ex.Message);
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "System error while retrieving applicant with ID: {ApplicantId}", request.Id);
                _loggingService.LogSystemError("GET_BY_ID", request.Id, ex);
                throw;
            }
        }

        private void ValidateInputSecurity(GetApplicantByIdQuery request)
        {
            // Validate applicant ID
            if (request.Id <= 0)
            {
                var errorMessage = $"Invalid applicant ID: {request.Id}";
                _loggingService.LogSecurityError("GET_BY_ID", request.Id, errorMessage);
                throw new InvalidOperationException(errorMessage);
            }

            // Check for SQL injection patterns
            if (_securityService.IsSqlInjectionAttempt(request.Id.ToString()))
            {
                var errorMessage = "Potential SQL injection detected in applicant ID";
                _loggingService.LogSecurityError("GET_BY_ID", request.Id, errorMessage);
                throw new InvalidOperationException(errorMessage);
            }
        }
    }
}