using MediatR;
using System;
using System.ComponentModel.DataAnnotations;

namespace ApplicantManagement.Application.Features.Applicants.Commands.DeleteApplicant
{
    public class DeleteApplicantCommand : IRequest<bool>
    {
        [Required(ErrorMessage = "Applicant ID is required")]
        [Range(1, int.MaxValue, ErrorMessage = "Invalid applicant ID")]
        public int Id { get; set; }

        [Required(ErrorMessage = "Row version is required for concurrency control")]
        public byte[] RowVersion { get; set; }

        public bool HardDelete { get; set; } = false;

        public string Reason { get; set; }

        public DeleteApplicantCommand(int id, byte[] rowVersion, bool hardDelete = false, string reason = null)
        {
            Id = id;
            RowVersion = rowVersion ?? throw new ArgumentNullException(nameof(rowVersion));
            HardDelete = hardDelete;
            Reason = reason;
        }
    }
}