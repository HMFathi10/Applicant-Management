using ApplicantManagement.Applicants.Services;
using ApplicantManagement.Domain.Entities;
using ApplicantManagement.Domain.Repositories;
using ApplicantManagement.Infrastructure.Repositories;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace ApplicantManagement.Application.Features.Applicants.Commands.CreateApplicant
{
    public class CreateApplicantHandler : IRequestHandler<CreateApplicantCommand, int>
    {
        private readonly IApplicantRepository _applicantRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<CreateApplicantHandler> _logger;
        private readonly IApplicantLoggingService _loggingService;
        private readonly IApplicantSecurityService _securityService;

        public CreateApplicantHandler(
            IApplicantRepository applicantRepository,
            IUnitOfWork unitOfWork,
            ILogger<CreateApplicantHandler> logger,
            IApplicantLoggingService loggingService,
            IApplicantSecurityService securityService)
        {
            _applicantRepository = applicantRepository ?? throw new ArgumentNullException(nameof(applicantRepository));
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _loggingService = loggingService ?? throw new ArgumentNullException(nameof(loggingService));
            _securityService = securityService ?? throw new ArgumentNullException(nameof(securityService));
        }

        public async Task<int> Handle(CreateApplicantCommand request, CancellationToken cancellationToken)
        {
            if (request == null)
            {
                throw new ArgumentNullException(nameof(request), "Create request cannot be null");
            }

            using var transactionScope = new System.Transactions.TransactionScope(
                System.Transactions.TransactionScopeOption.Required,
                System.Transactions.TransactionScopeAsyncFlowOption.Enabled);

            try
            {
                _logger.LogInformation("Starting create operation for applicant: {ApplicantName} {ApplicantFamilyName}", 
                    request.Name, request.FamilyName);

                // Validate input security
                ValidateInputSecurity(request);

                // Validate email uniqueness
                //var emailExists = await _applicantRepository.AnyAsync(a => 
                //    a.EmailAddress.ToLower() == request.EmailAddress.ToLower());
                var emailExists = await _applicantRepository.EmailExistsAsync(request.EmailAddress.ToLower());
                if (emailExists)
                {
                    _logger.LogWarning("Email address {Email} already exists", request.EmailAddress);
                    _loggingService.LogBusinessLogicError("CREATE", 0, $"Email address '{request.EmailAddress}' already exists");
                    throw new InvalidOperationException($"An applicant with email address '{request.EmailAddress}' already exists.");
                }

                // Validate age constraints
                if (request.Age < 18 || request.Age > 65)
                {
                    _logger.LogWarning("Invalid age provided: {Age}. Must be between 18 and 65", request.Age);
                    _loggingService.LogBusinessLogicError("CREATE", 0, $"Invalid age: {request.Age}. Must be between 18 and 65");
                    throw new InvalidOperationException("Applicant age must be between 18 and 65 years.");
                }

                // Validate applied date
                if (request.AppliedDate > DateTime.UtcNow)
                {
                    _logger.LogWarning("Applied date {AppliedDate} is in the future", request.AppliedDate);
                    _loggingService.LogBusinessLogicError("CREATE", 0, "Applied date cannot be in the future");
                    throw new InvalidOperationException("Applied date cannot be in the future.");
                }

                // Validate phone number format
                if (!string.IsNullOrWhiteSpace(request.Phone))
                {
                    var cleanedPhone = CleanPhoneNumber(request.Phone);
                    if (cleanedPhone.Length < 7 || cleanedPhone.Length > 15)
                    {
                        _logger.LogWarning("Invalid phone number format: {Phone}", request.Phone);
                        _loggingService.LogBusinessLogicError("CREATE", 0, $"Invalid phone number format: {request.Phone}");
                        throw new InvalidOperationException("Phone number must be between 7 and 15 digits.");
                    }
                }

                // Sanitize inputs
                var sanitizedName = _securityService.SanitizeInput(request.Name);
                var sanitizedFamilyName = _securityService.SanitizeInput(request.FamilyName);
                var sanitizedAddress = _securityService.SanitizeInput(request.Address);
                var sanitizedEmail = _securityService.SanitizeInput(request.EmailAddress.ToLower());
                var sanitizedPhone = _securityService.SanitizeInput(request.Phone);
                var sanitizedCountry = _securityService.SanitizeInput(request.CountryOfOrigin);

                var applicant = new Applicant
                {
                    Name = sanitizedName,
                    FamilyName = sanitizedFamilyName,
                    EmailAddress = sanitizedEmail,
                    Address = sanitizedAddress,
                    Phone = sanitizedPhone,
                    Age = request.Age,
                    CountryOfOrigin = sanitizedCountry,
                    AppliedDate = request.AppliedDate,
                    Hired = request.Hired,
                    CreatedDate = DateTime.UtcNow,
                    LastModifiedDate = DateTime.UtcNow,
                    CreatedBy = "System",
                    LastModifiedBy = "System",
                    IsDeleted = false
                };

                await _applicantRepository.AddAsync(applicant);
                await _unitOfWork.SaveChangesAsync();

                // Complete the transaction
                transactionScope.Complete();

                _loggingService.LogApplicantCreated(applicant.Id, $"{applicant.Name} {applicant.FamilyName}");
                _logger.LogInformation("Successfully created applicant with ID: {ApplicantId}, Email: {Email}", 
                    applicant.Id, applicant.EmailAddress);

                return applicant.Id;
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Business logic validation failed for applicant creation: {ApplicantName} {ApplicantFamilyName}", 
                    request.Name, request.FamilyName);
                _loggingService.LogBusinessLogicError("CREATE", 0, ex.Message);
                throw;
            }
            catch (ArgumentException ex)
            {
                _logger.LogError(ex, "Data validation error while creating applicant: {ApplicantName} {ApplicantFamilyName}", 
                    request.Name, request.FamilyName);
                _loggingService.LogBusinessLogicError("CREATE", 0, ex.Message);
                throw new InvalidOperationException(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error occurred while creating applicant: {ApplicantName} {ApplicantFamilyName}", 
                    request.Name, request.FamilyName);
                _loggingService.LogSystemError("CREATE", 0, ex);
                throw new InvalidOperationException($"Failed to create applicant: {ex.Message}", ex);
            }
        }

        private string CleanPhoneNumber(string phone)
        {
            if (string.IsNullOrWhiteSpace(phone))
                return string.Empty;

            // Remove all non-digit characters except +
            return System.Text.RegularExpressions.Regex.Replace(phone, @"[^\d+]", "");
        }

        private void ValidateInputSecurity(CreateApplicantCommand request)
        {
            // Validate email format
            if (!_securityService.IsValidEmail(request.EmailAddress))
            {
                var errorMessage = $"Invalid email format: {request.EmailAddress}";
                _loggingService.LogSecurityError("CREATE", 0, errorMessage);
                throw new InvalidOperationException(errorMessage);
            }

            //// Validate phone number
            //if (!string.IsNullOrWhiteSpace(request.Phone))
            //{
            //    if (!_securityService.IsValidPhoneNumber(request.Phone))
            //    {
            //        var errorMessage = $"Invalid phone number format: {request.Phone}";
            //        _loggingService.LogSecurityError("CREATE", 0, errorMessage);
            //        throw new InvalidOperationException(errorMessage);
            //    }
            //}

            // Validate names
            if (!_securityService.IsValidName(request.Name))
            {
                var errorMessage = $"Invalid name format: {request.Name}";
                _loggingService.LogSecurityError("CREATE", 0, errorMessage);
                throw new InvalidOperationException(errorMessage);
            }

            if (!_securityService.IsValidName(request.FamilyName))
            {
                var errorMessage = $"Invalid family name format: {request.FamilyName}";
                _loggingService.LogSecurityError("CREATE", 0, errorMessage);
                throw new InvalidOperationException(errorMessage);
            }

            // Validate address
            if (!_securityService.IsValidAddress(request.Address))
            {
                var errorMessage = $"Invalid address format: {request.Address}";
                _loggingService.LogSecurityError("CREATE", 0, errorMessage);
                throw new InvalidOperationException(errorMessage);
            }

            // Validate country
            if (!_securityService.IsValidCountry(request.CountryOfOrigin))
            {
                var errorMessage = $"Invalid country format: {request.CountryOfOrigin}";
                _loggingService.LogSecurityError("CREATE", 0, errorMessage);
                throw new InvalidOperationException(errorMessage);
            }

            // Check for SQL injection patterns
            if (_securityService.IsSqlInjectionAttempt(request.Name) ||
                _securityService.IsSqlInjectionAttempt(request.FamilyName) ||
                _securityService.IsSqlInjectionAttempt(request.EmailAddress) ||
                _securityService.IsSqlInjectionAttempt(request.Address) ||
                _securityService.IsSqlInjectionAttempt(request.Phone) ||
                _securityService.IsSqlInjectionAttempt(request.CountryOfOrigin))
            {
                var errorMessage = "Potential SQL injection detected in input data";
                _loggingService.LogSecurityError("CREATE", 0, errorMessage);
                throw new InvalidOperationException(errorMessage);
            }

            // Check for XSS attempts
            if (_securityService.IsXssAttempt(request.Name) ||
                _securityService.IsXssAttempt(request.FamilyName) ||
                _securityService.IsXssAttempt(request.EmailAddress) ||
                _securityService.IsXssAttempt(request.Address) ||
                _securityService.IsXssAttempt(request.Phone) ||
                _securityService.IsXssAttempt(request.CountryOfOrigin))
            {
                var errorMessage = "Potential XSS attack detected in input data";
                _loggingService.LogSecurityError("CREATE", 0, errorMessage);
                throw new InvalidOperationException(errorMessage);
            }
        }
    }
}