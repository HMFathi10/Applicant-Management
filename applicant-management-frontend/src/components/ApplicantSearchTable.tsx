import React, { useState, useEffect } from 'react';
import { applicantService } from '../services/applicantService';
import useDebounce from '../hooks/useDebounce';
import usePagination from '../hooks/usePagination';
import type { FrontendApplicant } from '../types';
import '../styles/ApplicantSearchTable.css';

interface PaginatedResponse<T> {
  items: T[];
  totalCount: number;
  currentPage: number;
  pageSize: number;
  totalPages: number;
  hasPreviousPage: boolean;
  hasNextPage: boolean;
}

const ApplicantSearchTable: React.FC = () => {
  // State for search input
  const [searchTerm, setSearchTerm] = useState<string>('');
  const debouncedSearchTerm = useDebounce(searchTerm, 500); // 500ms debounce delay
  
  // State for pagination
  const { page, pageSize, setPage, nextPage, prevPage } = usePagination();
  
  // State for applicants data
  const [applicantsData, setApplicantsData] = useState<PaginatedResponse<FrontendApplicant> | null>(null);
  const [loading, setLoading] = useState<boolean>(false);
  const [error, setError] = useState<string | null>(null);

  // Fetch applicants with server-side pagination and search
  useEffect(() => {
    const fetchApplicants = async () => {
      setLoading(true);
      setError(null);
      
      try {
        const response = await applicantService.searchApplicantsPaginated({
          searchTerm: debouncedSearchTerm,
          pageNumber: page,
          pageSize: pageSize,
          sortBy: 'Name',
          sortDescending: false
        });
        
        setApplicantsData(response);
      } catch (err) {
        setError('Failed to fetch applicants. Please try again.');
        console.error('Error fetching applicants:', err);
      } finally {
        setLoading(false);
      }
    };

    fetchApplicants();
  }, [debouncedSearchTerm, page, pageSize]); // Re-fetch when these values change

  // Handle search input change
  const handleSearchChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    setSearchTerm(e.target.value);
    setPage(1); // Reset to first page when search term changes
  };

  // Handle page size change
  const handlePageSizeChange = (e: React.ChangeEvent<HTMLSelectElement>) => {
    const newSize = parseInt(e.target.value, 10);
    setPage(1); // Reset to first page when page size changes
    usePagination().setPageSize(newSize);
  };

  return (
    <div className="applicant-search-table">
      <h2>Applicant Search with Server-side Pagination</h2>
      
      {/* Search input */}
      <div className="search-container">
        <input
          type="text"
          placeholder="Search applicants..."
          value={searchTerm}
          onChange={handleSearchChange}
          className="search-input"
        />
        <span className="search-info">
          {loading ? 'Searching...' : `Showing results for: ${debouncedSearchTerm || 'all applicants'}`}
        </span>
      </div>
      
      {/* Error message */}
      {error && <div className="error-message">{error}</div>}
      
      {/* Loading indicator */}
      {loading && <div className="loading-indicator">Loading...</div>}
      
      {/* Applicants table */}
      {!loading && applicantsData && (
        <>
          <table className="applicants-table">
            <thead>
              <tr>
                <th>ID</th>
                <th>Name</th>
                <th>Family Name</th>
                <th>Email</th>
                <th>Age</th>
                <th>Country</th>
                <th>Applied Date</th>
                <th>Status</th>
              </tr>
            </thead>
            <tbody>
              {applicantsData.items.length > 0 ? (
                applicantsData.items.map((applicant) => (
                  <tr key={applicant.id}>
                    <td>{applicant.id}</td>
                    <td>{applicant.name}</td>
                    <td>{applicant.familyName}</td>
                    <td>{applicant.emailAddress}</td>
                    <td>{applicant.age}</td>
                    <td>{applicant.countryOfOrigin}</td>
                    <td>{new Date(applicant.appliedDate).toLocaleDateString()}</td>
                    <td>{applicant.hired ? 'Hired' : 'Pending'}</td>
                  </tr>
                ))
              ) : (
                <tr>
                  <td colSpan={8} className="no-results">No applicants found</td>
                </tr>
              )}
            </tbody>
          </table>
          
          {/* Pagination controls */}
          <div className="pagination-controls">
            <div className="pagination-info">
              Showing {applicantsData.items.length} of {applicantsData.totalCount} applicants
            </div>
            <div className="pagination-actions">
              <button 
                onClick={prevPage} 
                disabled={!applicantsData.hasPreviousPage}
                className="pagination-button"
              >
                Previous
              </button>
              <span className="page-indicator">
                Page {applicantsData.currentPage} of {applicantsData.totalPages}
              </span>
              <button 
                onClick={nextPage} 
                disabled={!applicantsData.hasNextPage}
                className="pagination-button"
              >
                Next
              </button>
              <select 
                value={pageSize} 
                onChange={handlePageSizeChange}
                className="page-size-selector"
              >
                <option value="5">5 per page</option>
                <option value="10">10 per page</option>
                <option value="25">25 per page</option>
                <option value="50">50 per page</option>
              </select>
            </div>
          </div>
        </>
      )}
    </div>
  );
};

export default ApplicantSearchTable;