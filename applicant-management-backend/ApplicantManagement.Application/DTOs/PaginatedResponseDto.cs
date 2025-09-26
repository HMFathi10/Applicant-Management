using System.Collections.Generic;

namespace ApplicantManagement.Application.DTOs
{
    /// <summary>
    /// Generic paginated response DTO for API endpoints
    /// </summary>
    /// <typeparam name="T">Type of items in the paginated result</typeparam>
    public class PaginatedResponseDto<T>
    {
        /// <summary>
        /// The paginated items for the current page
        /// </summary>
        public List<T> Items { get; set; } = new List<T>();

        /// <summary>
        /// Total number of items across all pages
        /// </summary>
        public int TotalCount { get; set; }

        /// <summary>
        /// Current page number (1-based)
        /// </summary>
        public int CurrentPage { get; set; }

        /// <summary>
        /// Number of items per page
        /// </summary>
        public int PageSize { get; set; }

        /// <summary>
        /// Total number of pages
        /// </summary>
        public int TotalPages { get; set; }

        /// <summary>
        /// Whether there is a previous page
        /// </summary>
        public bool HasPreviousPage { get; set; }

        /// <summary>
        /// Whether there is a next page
        /// </summary>
        public bool HasNextPage { get; set; }

        public PaginatedResponseDto()
        {
        }

        public PaginatedResponseDto(List<T> items, int totalCount, int currentPage, int pageSize)
        {
            Items = items;
            TotalCount = totalCount;
            CurrentPage = currentPage;
            PageSize = pageSize;
            TotalPages = (int)System.Math.Ceiling(totalCount / (double)pageSize);
            HasPreviousPage = currentPage > 1;
            HasNextPage = currentPage < TotalPages;
        }
    }
}