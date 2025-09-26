using MediatR;
using System;
using System.ComponentModel.DataAnnotations;

namespace ApplicantManagement.Application.Features.Applicants.Commands.CreateApplicant
{
    public class CreateApplicantCommand : IRequest<int>
    {
        [Required]
        [StringLength(100, MinimumLength = 5)]
        public string Name { get; set; }
        
        [Required]
        [StringLength(100, MinimumLength = 5)]
        public string FamilyName { get; set; }
        
        [Required]
        [StringLength(255, MinimumLength = 10)]
        public string Address { get; set; }
        
        [Required]
        [RegularExpression(@"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$")]
        public string EmailAddress { get; set; }
        
        [Required]
        [StringLength(20, MinimumLength = 5)]
        public string Phone { get; set; }
        
        [Required]
        [Range(20, 60)]
        public int Age { get; set; }
        
        //public string Department { get; set; }
        
        [Required]
        public string CountryOfOrigin { get; set; }
        
        public DateTime AppliedDate { get; set; }
        
        public bool Hired { get; set; } = false;
    }
}