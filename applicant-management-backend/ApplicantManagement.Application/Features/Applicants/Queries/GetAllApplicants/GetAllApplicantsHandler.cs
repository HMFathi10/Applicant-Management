using ApplicantManagement.Domain.Entities;
using ApplicantManagement.Domain.Repositories;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace ApplicantManagement.Application.Features.Applicants.Queries.GetAllApplicants
{
    public class GetAllApplicantsHandler : IRequestHandler<GetAllApplicantsQuery, PaginatedApplicantResponse>
    {
        private readonly IRepository<Applicant> _applicantRepository;

        public GetAllApplicantsHandler(IRepository<Applicant> applicantRepository)
        {
            _applicantRepository = applicantRepository;
        }

        public async Task<PaginatedApplicantResponse> Handle(GetAllApplicantsQuery request, CancellationToken cancellationToken)
        {
            // Create filter expression
            Expression<Func<Applicant, bool>> filter = a => a.IsDeleted == false;
            
            // Add search filter if search term is provided
            if (!string.IsNullOrWhiteSpace(request.SearchTerm))
            {
                var searchTerm = request.SearchTerm.ToLower();
                filter = a => a.IsDeleted == false && (
                    a.Name.ToLower().Contains(searchTerm) ||
                    a.FamilyName.ToLower().Contains(searchTerm) ||
                    a.EmailAddress.ToLower().Contains(searchTerm)
                );
            }
            
            // Get total count of applicants matching the filter
            var totalCount = await _applicantRepository.CountAsync(filter);
            
            // Get paginated applicants
            var applicantsList = await _applicantRepository.GetPagedAsync(
                filter,
                pageNumber: request.Page,
                pageSize: request.PageSize,
                orderBy: q => q.OrderBy(a => a.Name)
            );
            
            // Create and return paginated response
            var response = new PaginatedApplicantResponse
            {
                Applicants = applicantsList,
                TotalCount = totalCount,
                Page = request.Page,
                PageSize = request.PageSize
            };
            
            return response;
        }
    }
}