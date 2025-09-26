using ApplicantManagement.Applicants.Services;
using ApplicantManagement.Domain.Entities;
using ApplicantManagement.Domain.Repositories;
using ApplicantManagement.Infrastructure.Repositories;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace ApplicantManagement.Application.Features.Applicants.Commands.UpdateApplicant
{
    public class UpdateApplicantHandler : IRequestHandler<UpdateApplicantCommand, bool>
    {
        private readonly IApplicantRepository _applicantRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<UpdateApplicantHandler> _logger;
        private readonly IApplicantLoggingService _loggingService;
        private readonly IApplicantSecurityService _securityService;

        public UpdateApplicantHandler(
            IApplicantRepository applicantRepository, 
            IUnitOfWork unitOfWork,
            ILogger<UpdateApplicantHandler> logger,
            IApplicantLoggingService loggingService,
            IApplicantSecurityService securityService)
        {
            _applicantRepository = applicantRepository ?? throw new ArgumentNullException(nameof(applicantRepository));
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _loggingService = loggingService ?? throw new ArgumentNullException(nameof(loggingService));
            _securityService = securityService ?? throw new ArgumentNullException(nameof(securityService));
        }

        public async Task<bool> Handle(UpdateApplicantCommand request, CancellationToken cancellationToken)
        {
            if (request == null)
            {
                throw new ArgumentNullException(nameof(request), "Update request cannot be null");
            }

            using var transactionScope = new System.Transactions.TransactionScope(
                System.Transactions.TransactionScopeOption.Required,
                System.Transactions.TransactionScopeAsyncFlowOption.Enabled);

            try
            {
                _logger.LogInformation("Starting update operation for applicant ID: {ApplicantId}", request.Id);

                // Validate input security
                ValidateInputSecurity(request);

                // Retrieve the existing applicant with enhanced repository
                var existingApplicant = await _applicantRepository.GetByIdAsync(request.Id);
                if (existingApplicant == null)
                {
                    _logger.LogWarning("Applicant with ID {ApplicantId} not found for update", request.Id);
                    _loggingService.LogBusinessLogicError("Update", request.Id, "Applicant not found");
                    return false;
                }

                // Check for optimistic concurrency (RowVersion)
                if (request.RowVersion != null && existingApplicant.RowVersion != null)
                {
                    if (!AreRowVersionsEqual(request.RowVersion, existingApplicant.RowVersion))
                    {
                        _logger.LogWarning("Concurrency conflict detected for applicant ID: {ApplicantId}. Request version: {RequestVersion}, Current version: {CurrentVersion}", 
                            request.Id, Convert.ToBase64String(request.RowVersion), Convert.ToBase64String(existingApplicant.RowVersion));
                        _loggingService.LogBusinessLogicError("Update", request.Id, "Concurrency conflict detected");
                        throw new InvalidOperationException("The applicant has been modified by another user. Please refresh and try again.");
                    }
                }

                // Validate email uniqueness (excluding current applicant) using enhanced repository
                var emailExists = await _applicantRepository.EmailExistsAsync(request.EmailAddress, request.Id);
                
                if (emailExists)
                {
                    _logger.LogWarning("Email address {Email} already exists for another applicant", request.EmailAddress);
                    _loggingService.LogBusinessLogicError("Update", request.Id, $"Email address {request.EmailAddress} already exists for another applicant");
                    throw new InvalidOperationException($"An applicant with email address '{request.EmailAddress}' already exists.");
                }

                // Validate age using repository method
                var isValidAge = await _applicantRepository.ValidateAgeAsync(request.Age);
                if (!isValidAge)
                {
                    _logger.LogWarning("Invalid age: {Age}", request.Age);
                    _loggingService.LogBusinessLogicError("Update", request.Id, $"Invalid age: {request.Age}. Age must be between 18 and 65");
                    throw new InvalidOperationException("Age must be between 18 and 65");
                }

                // Validate applied date using repository method
                var isValidAppliedDate = await _applicantRepository.ValidateAppliedDateAsync(request.AppliedDate);
                if (!isValidAppliedDate)
                {
                    _logger.LogWarning("Applied date is in the future: {AppliedDate}", request.AppliedDate);
                    _loggingService.LogBusinessLogicError("Update", request.Id, $"Applied date is in the future: {request.AppliedDate}");
                    throw new InvalidOperationException("Applied date cannot be in the future");
                }

                // Update applicant properties
                existingApplicant.Name = _securityService.SanitizeInput(request.Name);
                existingApplicant.FamilyName = _securityService.SanitizeInput(request.FamilyName);
                existingApplicant.EmailAddress = _securityService.SanitizeInput(request.EmailAddress.ToLower());
                existingApplicant.Address = _securityService.SanitizeInput(request.Address);
                existingApplicant.Phone = _securityService.SanitizeInput(request.Phone);
                existingApplicant.Age = request.Age;
                existingApplicant.CountryOfOrigin = _securityService.SanitizeInput(request.CountryOfOrigin);
                existingApplicant.AppliedDate = request.AppliedDate;
                existingApplicant.Hired = request.Hired;
                existingApplicant.LastModifiedDate = DateTime.UtcNow;

                // Perform the update using enhanced repository
                await _applicantRepository.UpdateAsync(existingApplicant);
                await _unitOfWork.SaveChangesAsync();

                // Complete the transaction
                transactionScope.Complete();

                _logger.LogInformation("Successfully updated applicant ID: {ApplicantId}", request.Id);
                _loggingService.LogApplicantUpdated(request.Id, $"{request.Name} {request.FamilyName}");
                return true;
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Business logic validation failed for applicant ID: {ApplicantId}", request.Id);
                _loggingService.LogBusinessLogicError("Update", request.Id, ex.Message, request);
                throw;
            }
            catch (ArgumentException ex)
            {
                _logger.LogError(ex, "Data validation error while updating applicant ID: {ApplicantId}", request.Id);
                _loggingService.LogBusinessLogicError("Update", request.Id, ex.Message, request);
                throw new InvalidOperationException(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error occurred while updating applicant ID: {ApplicantId}", request.Id);
                _loggingService.LogSystemError("Update", request.Id, ex, request);
                throw new InvalidOperationException($"Failed to update applicant: {ex.Message}", ex);
            }
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

        private void ValidateInputSecurity(UpdateApplicantCommand request)
        {
            // Validate email format
            if (!_securityService.IsValidEmail(request.EmailAddress))
            {
                _loggingService.LogValidationError("Update", request.Id, "Invalid email format", request);
                throw new InvalidOperationException("Invalid email address format");
            }

            // Validate phone number
            if (!_securityService.IsValidPhoneNumber(request.Phone))
            {
                _loggingService.LogValidationError("Update", request.Id, "Invalid phone number format", request);
                throw new InvalidOperationException("Invalid phone number format");
            }

            // Validate names
            if (!_securityService.IsValidName(request.Name))
            {
                _loggingService.LogValidationError("Update", request.Id, "Invalid name format", request);
                throw new InvalidOperationException("Invalid name format");
            }

            if (!_securityService.IsValidName(request.FamilyName))
            {
                _loggingService.LogValidationError("Update", request.Id, "Invalid family name format", request);
                throw new InvalidOperationException("Invalid family name format");
            }

            // Validate address
            if (!_securityService.IsValidAddress(request.Address))
            {
                _loggingService.LogValidationError("Update", request.Id, "Invalid address format", request);
                throw new InvalidOperationException("Invalid address format");
            }

            // Validate country
            if (!_securityService.IsValidCountry(request.CountryOfOrigin))
            {
                _loggingService.LogValidationError("Update", request.Id, "Invalid country format", request);
                throw new InvalidOperationException("Invalid country format");
            }

            // Check for SQL injection attempts
            if (_securityService.IsSqlInjectionAttempt(request.Name) ||
                _securityService.IsSqlInjectionAttempt(request.FamilyName) ||
                _securityService.IsSqlInjectionAttempt(request.EmailAddress) ||
                _securityService.IsSqlInjectionAttempt(request.Address) ||
                _securityService.IsSqlInjectionAttempt(request.CountryOfOrigin))
            {
                _loggingService.LogSecurityError("Update", request.Id, "Potential SQL injection attempt detected");
                throw new InvalidOperationException("Invalid input detected");
            }

            // Check for XSS attempts
            if (_securityService.IsXssAttempt(request.Name) ||
                _securityService.IsXssAttempt(request.FamilyName) ||
                _securityService.IsXssAttempt(request.EmailAddress) ||
                _securityService.IsXssAttempt(request.Address) ||
                _securityService.IsXssAttempt(request.CountryOfOrigin))
            {
                _loggingService.LogSecurityError("Update", request.Id, "Potential XSS attempt detected");
                throw new InvalidOperationException("Invalid input detected");
            }
        }

        private string SanitizeInput(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return input;

            // Remove potentially dangerous characters
            var sanitized = input.Trim()
                .Replace("<", "&lt;")
                .Replace(">", "&gt;")
                .Replace("\"", "&quot;")
                .Replace("'", "&#x27;");

            return sanitized;
        }
    }
}