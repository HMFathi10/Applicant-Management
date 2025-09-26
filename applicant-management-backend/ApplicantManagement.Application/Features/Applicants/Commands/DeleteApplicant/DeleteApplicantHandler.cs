using ApplicantManagement.Applicants.Services;
using ApplicantManagement.Domain.Entities;
using ApplicantManagement.Domain.Repositories;
using ApplicantManagement.Infrastructure.Repositories;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace ApplicantManagement.Application.Features.Applicants.Commands.DeleteApplicant
{
    public class DeleteApplicantHandler : IRequestHandler<DeleteApplicantCommand, bool>
    {
        private readonly IApplicantRepository _applicantRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<DeleteApplicantHandler> _logger;
        private readonly IApplicantLoggingService _loggingService;
        private readonly IApplicantSecurityService _securityService;

        public DeleteApplicantHandler(
            IApplicantRepository applicantRepository, 
            IUnitOfWork unitOfWork,
            ILogger<DeleteApplicantHandler> logger,
            IApplicantLoggingService loggingService,
            IApplicantSecurityService securityService)
        {
            _applicantRepository = applicantRepository ?? throw new ArgumentNullException(nameof(applicantRepository));
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _loggingService = loggingService ?? throw new ArgumentNullException(nameof(loggingService));
            _securityService = securityService ?? throw new ArgumentNullException(nameof(securityService));
        }

        public async Task<bool> Handle(DeleteApplicantCommand request, CancellationToken cancellationToken)
        {
            if (request == null)
            {
                throw new ArgumentNullException(nameof(request), "Delete request cannot be null");
            }

            using var transactionScope = new System.Transactions.TransactionScope(
                System.Transactions.TransactionScopeOption.Required,
                System.Transactions.TransactionScopeAsyncFlowOption.Enabled);

            try
            {
                _logger.LogInformation("Starting delete operation for applicant ID: {ApplicantId}, HardDelete: {HardDelete}", 
                    request.Id, request.HardDelete);

                // Validate input security
                ValidateInputSecurity(request);

                // Retrieve the existing applicant
                var applicant = await _applicantRepository.GetByIdAsync(request.Id);
                if (applicant == null)
                {
                    _logger.LogWarning("Applicant with ID {ApplicantId} not found for deletion", request.Id);
                    _loggingService.LogBusinessLogicError("DELETE", request.Id, $"Applicant not found for deletion");
                    return false;
                }

                // Check for optimistic concurrency (RowVersion)
                if (request.RowVersion != null && applicant.RowVersion != null)
                {
                    if (!AreRowVersionsEqual(request.RowVersion, applicant.RowVersion))
                    {
                        _logger.LogWarning("Concurrency conflict detected for applicant ID: {ApplicantId}. Request version: {RequestVersion}, Current version: {CurrentVersion}", 
                            request.Id, Convert.ToBase64String(request.RowVersion), Convert.ToBase64String(applicant.RowVersion));
                        _loggingService.LogBusinessLogicError("DELETE", request.Id, "Concurrency conflict detected during delete operation");
                        throw new InvalidOperationException("The applicant has been modified by another user. Please refresh and try again.");
                    }
                }

                if (request.HardDelete)
                {
                    // Permanent deletion
                    await PerformHardDelete(applicant, request);
                }
                else
                {
                    // Soft delete
                    await PerformSoftDelete(applicant, request);
                }

                // Complete the transaction
                transactionScope.Complete();

                return true;
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Business logic validation failed for applicant ID: {ApplicantId}", request.Id);
                _loggingService.LogBusinessLogicError("DELETE", request.Id, ex.Message);
                throw;
            }
            catch (ArgumentException ex)
            {
                _logger.LogError(ex, "Data validation error while deleting applicant ID: {ApplicantId}", request.Id);
                _loggingService.LogBusinessLogicError("DELETE", request.Id, ex.Message);
                throw new InvalidOperationException(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error occurred while deleting applicant ID: {ApplicantId}", request.Id);
                _loggingService.LogSystemError("DELETE", request.Id, ex);
                throw new InvalidOperationException($"Failed to delete applicant: {ex.Message}", ex);
            }
        }

        private async Task PerformHardDelete(Applicant applicant, DeleteApplicantCommand request)
        {
            _logger.LogInformation("Performing hard delete for applicant ID: {ApplicantId}, Reason: {Reason}", 
                request.Id, request.Reason ?? "No reason provided");

            await _applicantRepository.DeleteAsync(applicant);
            await _unitOfWork.SaveChangesAsync();

            _loggingService.LogApplicantDeleted(request.Id, $"{applicant.Name} {applicant.FamilyName}", request.HardDelete, request.Reason);
            _logger.LogInformation("Successfully performed hard delete for applicant ID: {ApplicantId}", request.Id);
        }

        private async Task PerformSoftDelete(Applicant applicant, DeleteApplicantCommand request)
        {
            _logger.LogInformation("Performing soft delete for applicant ID: {ApplicantId}, Reason: {Reason}", 
                request.Id, request.Reason ?? "No reason provided");

            // Mark as deleted instead of actually deleting
            applicant.IsDeleted = true;
            applicant.DeletedDate = DateTime.UtcNow;
            applicant.DeletedReason = request.Reason ?? "User requested deletion";
            applicant.LastModifiedDate = DateTime.UtcNow;

            await _applicantRepository.UpdateAsync(applicant);
            await _unitOfWork.SaveChangesAsync();

            _loggingService.LogApplicantDeleted(request.Id, $"{applicant.Name} {applicant.FamilyName}", request.HardDelete, request.Reason);
            _logger.LogInformation("Successfully performed soft delete for applicant ID: {ApplicantId}", request.Id);
        }

        private bool AreRowVersionsEqual(byte[] version1, byte[] version2)
        {
            if (version1 == null || version2 == null)
                return false;
            
            if (version1.Length != version2.Length)
                return false;

            for (int i = 0; i < version1.Length; i++)
            {
                if (version1[i] != version2[i])
                    return false;
            }

            return true;
        }

        private void ValidateInputSecurity(DeleteApplicantCommand request)
        {
            // Validate ID
            if (request.Id <= 0)
            {
                var errorMessage = "Invalid applicant ID for deletion";
                _loggingService.LogSecurityError("DELETE", request.Id, errorMessage);
                throw new InvalidOperationException(errorMessage);
            }

            // Validate RowVersion if provided
            if (request.RowVersion != null)
            {
                if (request.RowVersion.Length == 0)
                {
                    var errorMessage = "Row version cannot be empty";
                    _loggingService.LogSecurityError("DELETE", request.Id, errorMessage);
                    throw new InvalidOperationException(errorMessage);
                }
            }

            // Validate Reason if provided
            if (!string.IsNullOrEmpty(request.Reason))
            {
                if (request.Reason.Length > 500)
                {
                    var errorMessage = "Deletion reason exceeds maximum length of 500 characters";
                    _loggingService.LogSecurityError("DELETE", request.Id, errorMessage);
                    throw new InvalidOperationException(errorMessage);
                }

                // Check for SQL injection patterns
                if (_securityService.IsSqlInjectionAttempt(request.Reason))
                {
                    var errorMessage = "Potential SQL injection detected in deletion reason";
                    _loggingService.LogSecurityError("DELETE", request.Id, errorMessage);
                    throw new InvalidOperationException(errorMessage);
                }

                // Check for XSS patterns
                if (_securityService.IsXssAttempt(request.Reason))
                {
                    var errorMessage = "Potential XSS attack detected in deletion reason";
                    _loggingService.LogSecurityError("DELETE", request.Id, errorMessage);
                    throw new InvalidOperationException(errorMessage);
                }
            }
        }
    }
}