using ApplicantManagement.Application.DTOs;
using ApplicantManagement.Domain.Entities;
using MediatR;
using System;
using System.Collections.Generic;

namespace ApplicantManagement.Application.Features.Applicants.Queries.GetApplicantsWithFilters
{
    public class GetApplicantsWithFiltersQuery : IRequest<PaginatedResponseDto<Applicant>>
    {
        // Search parameters
        public string? SearchTerm { get; set; }
        
        // Filter parameters
        public int? MinAge { get; set; }
        public int? MaxAge { get; set; }
        public string? CountryOfOrigin { get; set; }
        public bool? IsHired { get; set; }
        public DateTime? AppliedDateFrom { get; set; }
        public DateTime? AppliedDateTo { get; set; }
        
        // Sorting parameters
        public string? SortBy { get; set; } = "Name"; // Name, FamilyName, Age, AppliedDate, CountryOfOrigin
        public bool SortDescending { get; set; } = false;
        
        // Pagination parameters
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 50;
        
        // Include deleted records
        public bool IncludeDeleted { get; set; } = false;

        public GetApplicantsWithFiltersQuery()
        {
            // Default constructor
        }

        public GetApplicantsWithFiltersQuery(string searchTerm = null, int? minAge = null, int? maxAge = null, 
            string countryOfOrigin = null, bool? isHired = null, DateTime? appliedDateFrom = null, 
            DateTime? appliedDateTo = null, string sortBy = "Name", bool sortDescending = false, 
            int pageNumber = 1, int pageSize = 50, bool includeDeleted = false)
        {
            SearchTerm = searchTerm;
            MinAge = minAge;
            MaxAge = maxAge;
            CountryOfOrigin = countryOfOrigin;
            IsHired = isHired;
            AppliedDateFrom = appliedDateFrom;
            AppliedDateTo = appliedDateTo;
            SortBy = sortBy;
            SortDescending = sortDescending;
            PageNumber = pageNumber;
            PageSize = pageSize;
            IncludeDeleted = includeDeleted;
        }
    }
}