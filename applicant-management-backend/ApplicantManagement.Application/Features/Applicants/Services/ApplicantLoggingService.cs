using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace ApplicantManagement.Applicants.Services
{
    public interface IApplicantLoggingService
    {
        void LogApplicantCreated(int applicantId, string applicantName, string performedBy = null);
        void LogApplicantUpdated(int applicantId, string applicantName, string performedBy = null);
        void LogApplicantDeleted(int applicantId, string applicantName, bool hardDelete, string reason, string performedBy = null);
        void LogApplicantRetrieved(int applicantId, string applicantName, string performedBy = null);
        void LogApplicantSearch(string searchTerm, int resultCount, string performedBy = null);
        void LogApplicantFilter(string filterCriteria, int resultCount, string performedBy = null);
        void LogValidationError(string operation, int? applicantId, string errorMessage, object data = null);
        void LogBusinessLogicError(string operation, int? applicantId, string errorMessage, object data = null);
        void LogSystemError(string operation, int? applicantId, Exception exception, object data = null);
        void LogSecurityError(string operation, int? applicantId, string errorMessage, object data = null);
    }

    public class ApplicantLoggingService : IApplicantLoggingService
    {
        private readonly ILogger<ApplicantLoggingService> _logger;

        public ApplicantLoggingService(ILogger<ApplicantLoggingService> logger)
        {
            _logger = logger;
        }

        public void LogApplicantCreated(int applicantId, string applicantName, string performedBy = null)
        {
            var logData = new
            {
                ApplicantId = applicantId,
                ApplicantName = applicantName,
                PerformedBy = performedBy ?? "System",
                Timestamp = DateTime.UtcNow
            };
            
            _logger.LogInformation("Applicant created successfully: {@LogData}", logData);
        }

        public void LogApplicantUpdated(int applicantId, string applicantName, string performedBy = null)
        {
            var logData = new
            {
                ApplicantId = applicantId,
                ApplicantName = applicantName,
                PerformedBy = performedBy ?? "System",
                Timestamp = DateTime.UtcNow
            };
            
            _logger.LogInformation("Applicant updated successfully: {@LogData}", logData);
        }

        public void LogApplicantDeleted(int applicantId, string applicantName, bool hardDelete, string reason, string performedBy = null)
        {
            var logData = new
            {
                ApplicantId = applicantId,
                ApplicantName = applicantName,
                HardDelete = hardDelete,
                Reason = reason,
                PerformedBy = performedBy ?? "System",
                Timestamp = DateTime.UtcNow
            };
            
            _logger.LogInformation("Applicant deleted: {@LogData}", logData);
        }

        public void LogApplicantRetrieved(int applicantId, string applicantName, string performedBy = null)
        {
            var logData = new
            {
                ApplicantId = applicantId,
                ApplicantName = applicantName,
                PerformedBy = performedBy ?? "System",
                Timestamp = DateTime.UtcNow
            };
            
            _logger.LogDebug("Applicant retrieved: {@LogData}", logData);
        }

        public void LogApplicantSearch(string searchTerm, int resultCount, string performedBy = null)
        {
            var logData = new
            {
                SearchTerm = searchTerm,
                ResultCount = resultCount,
                PerformedBy = performedBy ?? "System",
                Timestamp = DateTime.UtcNow
            };
            
            _logger.LogInformation("Applicant search performed: {@LogData}", logData);
        }

        public void LogApplicantFilter(string filterCriteria, int resultCount, string performedBy = null)
        {
            var logData = new
            {
                FilterCriteria = filterCriteria,
                ResultCount = resultCount,
                PerformedBy = performedBy ?? "System",
                Timestamp = DateTime.UtcNow
            };
            
            _logger.LogInformation("Applicant filter applied: {@LogData}", logData);
        }

        public void LogValidationError(string operation, int? applicantId, string errorMessage, object data = null)
        {
            var logData = new
            {
                Operation = operation,
                ApplicantId = applicantId,
                ErrorMessage = errorMessage,
                Data = data != null ? JsonSerializer.Serialize(data) : null,
                Timestamp = DateTime.UtcNow
            };
            
            _logger.LogWarning("Validation error in applicant operation: {@LogData}", logData);
        }

        public void LogBusinessLogicError(string operation, int? applicantId, string errorMessage, object data = null)
        {
            var logData = new
            {
                Operation = operation,
                ApplicantId = applicantId,
                ErrorMessage = errorMessage,
                Data = data != null ? JsonSerializer.Serialize(data) : null,
                Timestamp = DateTime.UtcNow
            };
            
            _logger.LogWarning("Business logic error in applicant operation: {@LogData}", logData);
        }

        public void LogSystemError(string operation, int? applicantId, Exception exception, object data = null)
        {
            var logData = new
            {
                Operation = operation,
                ApplicantId = applicantId,
                ExceptionType = exception.GetType().Name,
                ExceptionMessage = exception.Message,
                StackTrace = exception.StackTrace,
                Data = data != null ? JsonSerializer.Serialize(data) : null,
                Timestamp = DateTime.UtcNow
            };
            
            _logger.LogError(exception, "System error in applicant operation: {@LogData}", logData);
        }

        public void LogSecurityError(string operation, int? applicantId, string errorMessage, object data = null)
        {
            var logData = new
            {
                Operation = operation,
                ApplicantId = applicantId,
                ErrorMessage = errorMessage,
                Severity = "Security",
                Data = data != null ? JsonSerializer.Serialize(data) : null,
                Timestamp = DateTime.UtcNow
            };
            
            _logger.LogWarning("Security issue in applicant operation: {@LogData}", logData);
        }
    }
}