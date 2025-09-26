import React, { useState, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import ApplicantSearch from './ApplicantSearch';
import PaginationControls from './PaginationControls';
import type { Applicant } from '../types/applicant';

interface ApplicantListProps {
  onEdit?: (applicant: Applicant) => void;
  onDelete?: (applicant: Applicant) => void;
  onView?: (applicant: Applicant) => void;
}

const ApplicantList: React.FC<ApplicantListProps> = ({ onEdit, onDelete, onView }) => {
  const navigate = useNavigate();
  const [applicants, setApplicants] = useState<Applicant[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [currentPage, setCurrentPage] = useState(1);
  const [pageSize, setPageSize] = useState(10);
  const [totalCount, setTotalCount] = useState(0);
  const [searchTerm, setSearchTerm] = useState('');

  // Calculate total pages
  const totalPages = Math.ceil(totalCount / pageSize);

  // Fetch applicants with pagination and search
  const fetchApplicants = async (page: number, size: number, search: string) => {
    setLoading(true);
    try {
      const response = await fetch(
        `http://localhost:5259/api/applicants?page=${page}&pageSize=${size}&searchTerm=${encodeURIComponent(search)}`
      );
      
      if (!response.ok) {
        throw new Error('Failed to fetch applicants');
      }
      
      const data = await response.json();
      setApplicants(data.applicants);
      setTotalCount(data.totalCount);
      setCurrentPage(data.page);
      setPageSize(data.pageSize);
    } catch (err) {
      setError(err instanceof Error ? err.message : 'An error occurred');
    } finally {
      setLoading(false);
    }
  };

  // Initial fetch
  useEffect(() => {
    fetchApplicants(currentPage, pageSize, searchTerm);
  }, [currentPage, pageSize, searchTerm]);

  // Handle page change
  const handlePageChange = (page: number) => {
    setCurrentPage(page);
  };

  // Handle search
  const handleSearch = (term: string) => {
    setSearchTerm(term);
    setCurrentPage(1); // Reset to first page on new search
  };

  // Handle view applicant
  const handleViewApplicant = (applicant: Applicant) => {
    if (onView) {
      onView(applicant);
    } else {
      navigate(`/applicants/${applicant.id}`);
    }
  };

  // Handle edit applicant
  const handleEditApplicant = (applicant: Applicant) => {
    if (onEdit) {
      onEdit(applicant);
    } else {
      navigate(`/applicants/edit/${applicant.id}`);
    }
  };

  // Handle delete applicant
  const handleDeleteApplicant = (applicant: Applicant) => {
    if (onDelete) {
      onDelete(applicant);
    } else {
      if (window.confirm(`Are you sure you want to delete ${applicant.name} ${applicant.familyName}?`)) {
        // Implement delete logic
      }
    }
  };

  if (loading && applicants.length === 0) {
    return (
      <div className="loading-container">
        <div className="loading-spinner" />
        <p>Loading applicants...</p>
      </div>
    );
  }

  if (error) {
    return (
      <div className="error-container">
        <p className="error-message">{error}</p>
        <button onClick={() => fetchApplicants(currentPage, pageSize, searchTerm)}>
          Retry
        </button>
      </div>
    );
  }

  return (
    <div className="applicant-list-container">
      <div className="applicant-list-header">
        <h2>Applicants</h2>
        <ApplicantSearch onSearch={handleSearch} />
      </div>

      {applicants.length === 0 ? (
        <div className="no-applicants">
          <p>No applicants found matching your criteria.</p>
        </div>
      ) : (
        <>
          <div className="applicant-table-container">
            <table className="applicant-table">
              <thead>
                <tr>
                  <th>Name</th>
                  <th>Family Name</th>
                  <th>Email</th>
                  <th>Country</th>
                  <th>Hired</th>
                  <th>Actions</th>
                </tr>
              </thead>
              <tbody>
                {applicants.map((applicant) => (
                  <tr key={applicant.id}>
                    <td>{applicant.name}</td>
                    <td>{applicant.familyName}</td>
                    <td>{applicant.emailAddress}</td>
                    <td>{applicant.countryOfOrigin}</td>
                    <td>
                      <span className={`status-badge ${applicant.hired ? 'hired' : 'not-hired'}`}>
                        {applicant.hired ? 'Yes' : 'No'}
                      </span>
                    </td>
                    <td>
                      <div className="action-buttons">
                        <button 
                          className="action-button view" 
                          onClick={() => handleViewApplicant(applicant)}
                          aria-label={`View ${applicant.name}`}
                        >
                          View
                        </button>
                        <button 
                          className="action-button edit" 
                          onClick={() => handleEditApplicant(applicant)}
                          aria-label={`Edit ${applicant.name}`}
                        >
                          Edit
                        </button>
                        <button 
                          className="action-button delete" 
                          onClick={() => handleDeleteApplicant(applicant)}
                          aria-label={`Delete ${applicant.name}`}
                        >
                          Delete
                        </button>
                      </div>
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>

          <div className="pagination-container">
            <PaginationControls 
              currentPage={currentPage} 
              totalPages={totalPages} 
              onPageChange={handlePageChange} 
            />
          </div>
        </>
      )}
    </div>
  );
};

export default ApplicantList;