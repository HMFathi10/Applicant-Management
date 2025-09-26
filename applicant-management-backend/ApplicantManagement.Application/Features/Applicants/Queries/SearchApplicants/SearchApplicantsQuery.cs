using ApplicantManagement.Domain.Entities;
using MediatR;
using System.Collections.Generic;

namespace ApplicantManagement.Application.Features.Applicants.Queries.SearchApplicants
{
    public class SearchApplicantsQuery : IRequest<List<Applicant>>
    {
        public string SearchTerm { get; set; }

        public SearchApplicantsQuery(string searchTerm = null)
        {
            SearchTerm = searchTerm;
        }
    }
}