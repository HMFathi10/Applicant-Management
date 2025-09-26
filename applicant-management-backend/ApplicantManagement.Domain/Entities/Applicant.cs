using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace ApplicantManagement.Domain.Entities
{
    public class Applicant
    {
        [Key]
        public int Id { get; set; }
        
        [Required]
        [StringLength(100, MinimumLength = 5, ErrorMessage = "Name must be between 5 and 100 characters")]
        public string Name { get; set; }
        
        [Required]
        [StringLength(100, MinimumLength = 5, ErrorMessage = "Family name must be between 5 and 100 characters")]
        public string FamilyName { get; set; }
        
        [Required]
        [StringLength(255, MinimumLength = 10, ErrorMessage = "Address must be between 10 and 255 characters")]
        public string Address { get; set; }
        
        [Required]
        [RegularExpression(@"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$", 
            ErrorMessage = "Email address is not in a valid format")]
        public string EmailAddress { get; set; }
        
        [Required]
        [StringLength(20, MinimumLength = 5, ErrorMessage = "Phone must be between 5 and 20 characters")]
        public string Phone { get; set; }
        
        [Required]
        [Range(20, 60, ErrorMessage = "Age must be between 20 and 60")]
        public int Age { get; set; }
                
        [Required]
        public string CountryOfOrigin { get; set; }
        
        public DateTime AppliedDate { get; set; }
        
        [Required]
        public bool Hired { get; set; } = false;
        
        // Audit properties
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
        public DateTime? LastModifiedDate { get; set; }
        //[Required]
        public string CreatedBy { get; set; }
        //[Required]
        public string LastModifiedBy { get; set; }
        
        // Soft delete properties
        public bool IsDeleted { get; set; } = false;
        public DateTime? DeletedDate { get; set; }
        //[Required]
        public string? DeletedReason { get; set; }
        
        // Concurrency token
        [Required]
        [Timestamp]
        public byte[] RowVersion { get; set; }
    }
}