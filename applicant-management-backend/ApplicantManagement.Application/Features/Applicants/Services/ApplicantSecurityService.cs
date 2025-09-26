using System.Text.RegularExpressions;
using System.Web;

namespace ApplicantManagement.Applicants.Services
{
    public interface IApplicantSecurityService
    {
        string SanitizeInput(string input);
        bool IsValidEmail(string email);
        bool IsValidPhoneNumber(string phoneNumber);
        bool IsValidName(string name);
        bool IsValidAddress(string address);
        bool IsValidCountry(string country);
        bool IsSqlInjectionAttempt(string input);
        bool IsXssAttempt(string input);
        string SanitizeHtml(string htmlContent);
        string RemoveSpecialCharacters(string input, bool allowSpaces = true);
    }

    public class ApplicantSecurityService : IApplicantSecurityService
    {
        private static readonly Regex EmailRegex = new Regex(
            @"^[^@\s]+@[^@\s]+\.[^@\s]+$", 
            RegexOptions.Compiled | RegexOptions.IgnoreCase);
        
        private static readonly Regex PhoneRegex = new Regex(
            @"^\d{7,15}$", 
            RegexOptions.Compiled);
        
        private static readonly Regex NameRegex = new Regex(
            @"^[a-zA-Z\s\-\']+$", 
            RegexOptions.Compiled);
        
        private static readonly Regex SqlInjectionPattern = new Regex(
            @"(\b(union|select|insert|update|delete|drop|create|alter|exec|execute|script|declare|truncate)\b|['"";\\-])",
            RegexOptions.Compiled | RegexOptions.IgnoreCase);
        
        private static readonly Regex XssPattern = new Regex(
            @"<(script|iframe|object|embed|form|input|link|style|img|svg|math)[^>]*>|<[^>]*on\w+\s*=|javascript:|data:text/html|vbscript:",
            RegexOptions.Compiled | RegexOptions.IgnoreCase);

        public string SanitizeInput(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return input;

            // HTML encode to prevent XSS
            var sanitized = HttpUtility.HtmlEncode(input);
            
            // Remove any remaining potentially dangerous characters
            sanitized = RemoveDangerousCharacters(sanitized);
            
            // Trim and normalize whitespace
            sanitized = Regex.Replace(sanitized, @"\s+", " ").Trim();
            
            return sanitized;
        }

        public bool IsValidEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return false;
            
            return EmailRegex.IsMatch(email) && !IsSqlInjectionAttempt(email) && !IsXssAttempt(email);
        }

        public bool IsValidPhoneNumber(string phoneNumber)
        {
            if (string.IsNullOrWhiteSpace(phoneNumber))
                return false;
            
            return PhoneRegex.IsMatch(phoneNumber) && !IsSqlInjectionAttempt(phoneNumber);
        }

        public bool IsValidName(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                return false;
            
            return NameRegex.IsMatch(name) && !IsSqlInjectionAttempt(name) && !IsXssAttempt(name);
        }

        public bool IsValidAddress(string address)
        {
            if (string.IsNullOrWhiteSpace(address))
                return false;
            
            // Addresses can contain letters, numbers, spaces, and common punctuation
            var addressRegex = new Regex(@"^[a-zA-Z0-9\s\,\.\-\#]+$");
            
            return addressRegex.IsMatch(address) && !IsSqlInjectionAttempt(address) && !IsXssAttempt(address);
        }

        public bool IsValidCountry(string country)
        {
            if (string.IsNullOrWhiteSpace(country))
                return false;
            
            // Country names should only contain letters and spaces
            var countryRegex = new Regex(@"^[a-zA-Z\s\-\']+$");
            
            return countryRegex.IsMatch(country) && !IsSqlInjectionAttempt(country) && !IsXssAttempt(country);
        }

        public bool IsSqlInjectionAttempt(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return false;
            
            return SqlInjectionPattern.IsMatch(input);
        }

        public bool IsXssAttempt(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return false;
            
            return XssPattern.IsMatch(input);
        }

        public string SanitizeHtml(string htmlContent)
        {
            if (string.IsNullOrWhiteSpace(htmlContent))
                return htmlContent;
            
            // Remove all HTML tags
            var sanitized = Regex.Replace(htmlContent, @"<[^>]*>", string.Empty);
            
            // HTML encode the remaining content
            sanitized = HttpUtility.HtmlEncode(sanitized);
            
            return sanitized;
        }

        public string RemoveSpecialCharacters(string input, bool allowSpaces = true)
        {
            if (string.IsNullOrWhiteSpace(input))
                return input;
            
            var pattern = allowSpaces ? @"[^a-zA-Z0-9\s]" : @"[^a-zA-Z0-9]";
            return Regex.Replace(input, pattern, string.Empty);
        }

        private string RemoveDangerousCharacters(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return input;
            
            // Remove null bytes and other dangerous characters
            input = input.Replace("\0", string.Empty);
            input = input.Replace("\x1A", string.Empty); // Substitute character
            
            return input;
        }
    }
}