using ApplicantManagement.Domain.Entities;
using MediatR;
using System.Collections;
using System.Collections.Generic;

namespace ApplicantManagement.Application.Features.Applicants.Queries.GetAllApplicants
{
    public class GetAllApplicantsQuery : IRequest<PaginatedApplicantResponse>
    {
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public string SearchTerm { get; set; } = string.Empty;
    }

    public class PaginatedApplicantResponse : IEnumerable<Applicant>
    {
        public List<Applicant> Applicants { get; set; } = new List<Applicant>();
        public int TotalCount { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int? TotalPages { get; set; }
        public int? CurrentPage { get; set; }

        public IEnumerator<Applicant> GetEnumerator()
        {
            return Applicants.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}