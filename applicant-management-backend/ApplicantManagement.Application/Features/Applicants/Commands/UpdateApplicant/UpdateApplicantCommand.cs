using MediatR;
using System;
using System.ComponentModel.DataAnnotations;

namespace ApplicantManagement.Application.Features.Applicants.Commands.UpdateApplicant
{
    public class UpdateApplicantCommand : IRequest<bool>
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Name is required")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "Name must be between 2 and 100 characters")]
        [RegularExpression(@"^[a-zA-Z\s\-\']+$", ErrorMessage = "Name can only contain letters, spaces, hyphens, and apostrophes")]
        public string Name { get; set; }
        
        [Required(ErrorMessage = "Family name is required")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "Family name must be between 2 and 100 characters")]
        [RegularExpression(@"^[a-zA-Z\s\-\']+$", ErrorMessage = "Family name can only contain letters, spaces, hyphens, and apostrophes")]
        public string FamilyName { get; set; }
        
        [Required(ErrorMessage = "Address is required")]
        [StringLength(500, MinimumLength = 10, ErrorMessage = "Address must be between 10 and 500 characters")]
        public string Address { get; set; }
        
        [Required(ErrorMessage = "Email address is required")]
        [StringLength(255, ErrorMessage = "Email address cannot exceed 255 characters")]
        [RegularExpression(@"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$", ErrorMessage = "Invalid email address format")]
        public string EmailAddress { get; set; }
        
        [StringLength(20, ErrorMessage = "Phone number cannot exceed 20 characters")]
        [RegularExpression(@"^[\+]?[0-9\s\-\(\)]{7,20}$", ErrorMessage = "Invalid phone number format")]
        public string Phone { get; set; }
        
        [Required(ErrorMessage = "Age is required")]
        [Range(18, 65, ErrorMessage = "Age must be between 18 and 65")]
        public int Age { get; set; }
        
        [Required(ErrorMessage = "Country of origin is required")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "Country name must be between 2 and 100 characters")]
        public string CountryOfOrigin { get; set; }
        
        public DateTime AppliedDate { get; set; }
        
        public bool Hired { get; set; }

        [Timestamp]
        public byte[] RowVersion { get; set; }
    }
}