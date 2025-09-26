using MediatR;
using ApplicantManagement.Domain.Entities;

namespace ApplicantManagement.Application.Features.Applicants.Queries.GetApplicantById
{
    public class GetApplicantByIdQuery : IRequest<Applicant>
    {
        public int Id { get; set; }

        public GetApplicantByIdQuery(int id)
        {
            Id = id;
        }
    }
}