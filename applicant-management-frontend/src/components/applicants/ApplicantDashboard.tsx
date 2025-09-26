import React, { useState, useMemo, useEffect, useRef, useCallback } from 'react';
import { useNavigate } from 'react-router-dom';
import { useApplicantContext } from '../../context/ApplicantContext';
import type { Applicant } from '../../types';
import { Button, Input, Card, Pagination } from '../common';
import { Search, Filter, Plus, Eye, Edit, Trash2, Download, Upload } from 'lucide-react';
import useDebounce from '../../hooks/useDebounce';

export const ApplicantDashboard: React.FC = () => {
  const { state, actions } = useApplicantContext();
  const navigate = useNavigate();

  const [searchQuery, setSearchQuery] = useState('');
  const [showFilters, setShowFilters] = useState(false);
  const [loading, setLoading] = useState(false);
  const searchInputRef = useRef<HTMLInputElement>(null);
  
  // Use debounced search term
  const debouncedSearchTerm = useDebounce(searchQuery, 500);
  
  // Memoize the fetchApplicants function to prevent unnecessary re-renders
  const fetchApplicants = useCallback(async () => {
    console.log('ApplicantDashboard: fetchApplicants called with:', {
      currentPage: state.currentPage,
      itemsPerPage: state.itemsPerPage,
      debouncedSearchTerm: debouncedSearchTerm
    });
    setLoading(true);
    try {
      await actions.fetchPaginatedApplicants(
        state.currentPage,
        state.itemsPerPage,
        debouncedSearchTerm
      );
      console.log('ApplicantDashboard: fetchApplicants completed successfully');
    } catch (error) {
      console.error('Failed to fetch applicants:', error);
    } finally {
      setLoading(false);
    }
  }, [state.currentPage, state.itemsPerPage, debouncedSearchTerm, actions.fetchPaginatedApplicants]);
  
  // Fetch applicants when pagination params or search term changes
  useEffect(() => {
    console.log('ApplicantDashboard: Fetching applicants with params:', {
      currentPage: state.currentPage,
      itemsPerPage: state.itemsPerPage,
      debouncedSearchTerm: debouncedSearchTerm
    });
    fetchApplicants();
  }, [state.currentPage, state.itemsPerPage, debouncedSearchTerm]); // Keep original dependencies to avoid circular dependency
  
  // Use the applicants directly from state
  const paginatedApplicants = state.applicants;
  
  // Total pages is now calculated from the server response
  const totalPages = state.totalPages;

  // Initialize search query from context only on mount
  useEffect(() => {
    setSearchQuery(state.searchTerm || '');
  }, []); // Only run on mount

  // Auto-focus search input on component mount only
  useEffect(() => {
    if (searchInputRef.current) {
      searchInputRef.current.focus();
    }
  }, []); // Empty dependency array - only run on mount

  // Calculate stats
  const stats = useMemo(() => {
    console.log('ApplicantDashboard: Current applicants in state:', state.applicants);
    console.log('ApplicantDashboard: Current state:', {
      applicants: state.applicants,
      totalItems: state.totalItems,
      currentPage: state.currentPage,
      itemsPerPage: state.itemsPerPage,
      totalPages: state.totalPages
    });
    const total = state.totalItems || state.applicants.length; // Use totalItems from server

    return {
      total,
      newThisWeek: 0, // Placeholder - would need date logic
      hired: state.applicants.filter(app => app.hired).length
    };
  }, [state.applicants, state.totalItems]);

  // Handle search - simplified to prevent excessive updates
  const handleSearch = useCallback((query: string) => {
    setSearchQuery(query);
    // Update the search term in context only when debounced value changes
    if (query !== state.searchTerm) {
      actions.setSearchTerm(query);
      // Reset to first page when search changes
      if (state.currentPage !== 1) {
        actions.setCurrentPage(1);
      }
    }
  }, [actions, state.searchTerm, state.currentPage]);

  // Handle filter reset
  const handleFilterReset = useCallback(() => {
    setSearchQuery('');
    actions.resetFilters();
    setShowFilters(false);
    // Reset to first page when filters are reset
    if (state.currentPage !== 1) {
      actions.setCurrentPage(1);
    }
  }, [actions, state.currentPage]);

  // Handle export
  const handleExport = useCallback((format: 'csv' | 'json') => {
    try {
      const timestamp = new Date().toISOString().split('T')[0];
      const filename = `applicants-${timestamp}.${format}`;

      if (format === 'csv') {
        const csvContent = [
          ['Name', 'Family Name', 'Phone', 'Country', 'Address', 'Hired'].join(','),
          ...state.applicants.map(app => [
            `"${app.name || ''}"`,
            `"${app.familyName || ''}"`,
            `"${app.phone || ''}"`,
            `"${app.countryOfOrigin || ''}"`,
            `"${app.address || ''}"`,
            `"${app.hired ? 'Yes' : 'No'}"`
          ].join(','))
        ].join('\n');

        const blob = new Blob([csvContent], { type: 'text/csv' });
        const url = URL.createObjectURL(blob);
        const link = document.createElement('a');
        link.href = url;
        link.download = filename;
        link.click();
        URL.revokeObjectURL(url);
      } else {
        const jsonContent = JSON.stringify(state.applicants, null, 2);
        const blob = new Blob([jsonContent], { type: 'application/json' });
        const url = URL.createObjectURL(blob);
        const link = document.createElement('a');
        link.href = url;
        link.download = filename;
        link.click();
        URL.revokeObjectURL(url);
      }
    } catch (error) {
      console.error('Export failed:', error);
      alert('Export failed. Please try again.');
    }
  }, [state.applicants]);

  // Handle applicant actions
  const handleViewApplicant = useCallback((applicant: Applicant) => {
    navigate(`/applicants/${applicant.id}`);
  }, [navigate]);

  const handleEditApplicant = useCallback((applicant: Applicant) => {
    navigate(`/applicants/${applicant.id}/edit`);
  }, [navigate]);

  const handleDeleteApplicant = useCallback(async (applicant: Applicant) => {
    if (window.confirm(`Are you sure you want to delete ${applicant.name}?`)) {
      try {
        await actions.deleteApplicant(applicant.id);
      } catch (error) {
        console.error('Delete failed:', error);
        alert('Failed to delete applicant. Please try again.');
      }
    }
  }, [actions]);

  const handleAddApplicant = useCallback(() => {
    navigate('/applicants/new');
  }, [navigate]);

  // Handle sort change
  const handleSortChange = useCallback((e: React.ChangeEvent<HTMLSelectElement>) => {
    const [field, order] = e.target.value.split('-');
    actions.setSort(field, order as 'asc' | 'desc');
  }, [actions]);

  // Handle pagination change
  const handlePageChange = useCallback((page: number) => {
    actions.setCurrentPage(page);
  }, [actions]);

  const handleItemsPerPageChange = useCallback((itemsPerPage: number) => {
    actions.setItemsPerPage(itemsPerPage);
  }, [actions]);

  // Show loading state only when actually loading
  if (state.loading && !loading && state.applicants.length === 0) {
    return (
      <div className="container" style={{ padding: '2rem 0' }}>
        <div style={{ textAlign: 'center', padding: '4rem 0' }}>
          <div className="loading-spinner" />
          <p>Loading applicants...</p>
        </div>
      </div>
    );
  }

  if (state.error) {
    return (
      <div className="container" style={{ padding: '2rem 0' }}>
        <Card>
          <div style={{ textAlign: 'center', padding: '2rem' }}>
            <p style={{ color: 'var(--color-error)', marginBottom: '1rem' }}>
              {state.error}
            </p>
            <Button onClick={fetchApplicants}>
              Retry
            </Button>
          </div>
        </Card>
      </div>
    );
  }

  return (
    <div className="container" style={{ padding: '2rem 0' }} role="main" aria-label="Applicant Management Dashboard">
      {/* Header */}
      <div style={{ marginBottom: '2rem' }}>
        <h1 style={{ marginBottom: '0.5rem' }}>Applicant Management</h1>
        <p style={{ color: 'var(--color-text-secondary)' }}>
          Manage and track job applicants across your organization
        </p>
      </div>

      {/* Controls */}
      <div style={{ marginBottom: '2rem' }} role="search" aria-label="Search and filter applicants">
        <div style={{ display: 'flex', gap: '1rem', flexWrap: 'wrap', alignItems: 'center' }}>
          <div style={{ flex: '1', minWidth: '250px' }}>
            <Input
              ref={searchInputRef}
              icon={<Search size={16} />}
              placeholder="Search by name, family name, phone, country, or address..."
              value={searchQuery}
              onChange={(e) => handleSearch(e.target.value)}
              aria-label="Search applicants by name, family name, phone, country, or address"
            />
          </div>

          <Button icon={<Plus size={16} />} onClick={handleAddApplicant}>
            Add Applicant
          </Button>
          <Button
            variant="outline"
            icon={<Download size={16} />}
            onClick={() => handleExport('csv')}
            aria-label="Export applicants to CSV"
          >
            Export CSV
          </Button>
        </div>

        {/* Filter Panel */}
        {showFilters && (
          <div style={{ marginTop: '1rem' }}>
            <Card>
              <div style={{ padding: '1.5rem' }}>
                <h3 style={{ marginBottom: '1.5rem' }}>Filter & Sort Applicants</h3>
                <div style={{ display: 'grid', gap: '1rem', gridTemplateColumns: 'repeat(auto-fit, minmax(200px, 1fr))' }}>
                  <div>
                    <label style={{ display: 'block', marginBottom: '0.5rem', fontWeight: '500' }}>
                      Sort By
                    </label>
                    <select
                      value={`${state.sortBy}-${state.sortOrder}`}
                      onChange={handleSortChange}
                      style={{
                        width: '100%',
                        padding: '0.5rem',
                        border: '1px solid var(--color-border)',
                        borderRadius: 'var(--border-radius-sm)',
                        backgroundColor: 'var(--color-background)',
                        color: 'black',
                      }}
                      aria-label="Sort applicants"
                    >
                      <option value="name-asc">Name (A-Z)</option>
                    </select>
                  </div>
                </div>
                <div style={{ display: 'flex', gap: '1rem', justifyContent: 'space-between', marginTop: '1.5rem' }}>
                  <div style={{ display: 'flex', gap: '1rem' }}>
                    <Button
                      variant="outline"
                      icon={<Download size={16} />}
                      onClick={() => handleExport('csv')}
                      aria-label="Export filtered results to CSV"
                    >
                      Export CSV
                    </Button>
                    <Button
                      variant="outline"
                      icon={<Upload size={16} />}
                      onClick={() => handleExport('json')}
                      aria-label="Export filtered results to JSON"
                    >
                      Export JSON
                    </Button>
                  </div>
                  <div style={{ display: 'flex', gap: '1rem' }}>
                    <Button variant="outline" onClick={handleFilterReset}>
                      Reset Filters
                    </Button>
                    <Button onClick={() => setShowFilters(false)}>
                      Close
                    </Button>
                  </div>
                </div>
              </div>
            </Card>
          </div>
        )}
      </div>

      {/* Stats */}
      <div style={{ display: 'grid', gap: '1rem', gridTemplateColumns: 'repeat(auto-fit, minmax(200px, 1fr))', marginBottom: '2rem' }} role="region" aria-label="Application statistics">
        <Card>
          <div style={{ padding: '1.5rem', textAlign: 'center' }}>
            <h3 style={{ fontSize: '2rem', color: 'var(--color-primary)', marginBottom: '0.5rem' }} aria-label={`${stats.total} total applicants`}>
              {loading ? '...' : stats.total}
            </h3>
            <p style={{ color: 'var(--color-text-secondary)' }}>Total Applicants</p>
          </div>
        </Card>
        <Card>
          <div style={{ padding: '1.5rem', textAlign: 'center' }}>
            <h3 style={{ fontSize: '2rem', color: 'var(--color-status-warning)', marginBottom: '0.5rem' }} aria-label={`${stats.hired} applicants hired`}>
              {loading ? '...' : stats.hired}
            </h3>
            <p style={{ color: 'var(--color-text-secondary)' }}>Hired</p>
          </div>
        </Card>
      </div>

      {/* Applicant Datatable */}
      <div role="region" aria-label="Applicant data table">
        <Card>
          {loading && (
            <div style={{ 
              position: 'absolute', 
              top: 0, 
              left: 0, 
              right: 0, 
              bottom: 0, 
              backgroundColor: 'rgba(255, 255, 255, 0.8)', 
              display: 'flex', 
              alignItems: 'center', 
              justifyContent: 'center',
              zIndex: 1
            }}>
              <div className="loading-spinner" />
            </div>
          )}
          <div style={{ overflowX: 'auto', position: 'relative' }}>
            <table style={{ width: '100%', borderCollapse: 'collapse', fontSize: '0.9rem' }}>
              <thead>
                <tr style={{ borderBottom: '1px solid var(--color-border)', backgroundColor: 'var(--color-surface)' }}>
                  <th style={{ padding: '0.75rem 1rem', textAlign: 'left' }}>Name</th>
                  <th style={{ padding: '0.75rem 1rem', textAlign: 'left' }}>Family Name</th>
                  <th style={{ padding: '0.75rem 1rem', textAlign: 'left' }}>Phone</th>
                  <th style={{ padding: '0.75rem 1rem', textAlign: 'left' }}>Country</th>
                  <th style={{ padding: '0.75rem 1rem', textAlign: 'left' }}>Address</th>
                  <th style={{ padding: '0.75rem 1rem', textAlign: 'left' }}>Hired</th>
                  <th style={{ padding: '0.75rem 1rem', textAlign: 'center' }}>Actions</th>
                </tr>
              </thead>
              <tbody>
                {paginatedApplicants.map((applicant) => (
                  <tr 
                    key={applicant.id} 
                    style={{ borderBottom: '1px solid var(--color-border)' }}
                    role="row"
                    aria-label={`${applicant.name} ${applicant.familyName}`}
                  >
                    <td style={{ padding: '0.75rem 1rem' }}>{applicant.name}</td>
                    <td style={{ padding: '0.75rem 1rem' }}>{applicant.familyName}</td>
                    <td style={{ padding: '0.75rem 1rem' }}>{applicant.phone}</td>
                    <td style={{ padding: '0.75rem 1rem' }}>{applicant.countryOfOrigin}</td>
                    <td style={{ padding: '0.75rem 1rem' }}>{applicant.address}</td>
                    <td style={{ padding: '0.75rem 1rem' }}>
                      <span style={{
                        display: 'inline-block',
                        padding: '0.25rem 0.75rem',
                        borderRadius: 'var(--border-radius-sm)',
                        fontSize: '0.8rem',
                        fontWeight: '500',
                        backgroundColor: applicant.hired ? 'var(--color-status-success-bg)' : 'var(--color-status-warning-bg)',
                        color: applicant.hired ? 'var(--color-status-success)' : 'var(--color-status-warning)'
                      }}>
                        {applicant.hired ? 'Yes' : 'No'}
                      </span>
                    </td>
                    <td style={{ padding: '0.75rem 1rem', textAlign: 'center' }}>
                      <div style={{ display: 'flex', justifyContent: 'center', gap: '0.5rem' }}>
                        <Button
                          variant="ghost"
                          size="sm"
                          icon={<Eye size={16} />}
                          onClick={() => handleViewApplicant(applicant)}
                          title="View Details"
                          aria-label={`View details for ${applicant.name} ${applicant.familyName}`}
                        />
                        <Button
                          variant="ghost"
                          size="sm"
                          icon={<Edit size={16} />}
                          onClick={() => handleEditApplicant(applicant)}
                          title="Edit Applicant"
                          aria-label={`Edit ${applicant.name} ${applicant.familyName}`}
                        />
                        <Button
                          variant="ghost"
                          size="sm"
                          icon={<Trash2 size={16} />}
                          onClick={() => handleDeleteApplicant(applicant)}
                          title="Delete Applicant"
                          style={{ color: 'var(--color-error)' }}
                          aria-label={`Delete ${applicant.name} ${applicant.familyName}`}
                        />
                      </div>
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        </Card>
      </div>

      {state.applicants.length === 0 && !state.loading && !loading && (
        <Card>
          <div style={{ textAlign: 'center', padding: '3rem' }} role="status" aria-live="polite">
            <p style={{ color: 'var(--color-text-secondary)', marginBottom: '1rem' }}>
              No applicants found matching your criteria.
            </p>
            {state.searchTerm ? (
              <Button onClick={handleFilterReset} aria-label="Clear search filter">
                Clear Search
              </Button>
            ) : (
              <Button onClick={handleAddApplicant} icon={<Plus size={16} />} aria-label="Add first applicant">
                Add First Applicant
              </Button>
            )}
          </div>
        </Card>
      )}

      {/* Pagination */}
      {state.applicants.length > 0 && (
        <div style={{ marginTop: '2rem' }}>
          <Pagination
            currentPage={state.currentPage}
            totalPages={totalPages}
            itemsPerPage={state.itemsPerPage}
            totalItems={state.totalItems}
            onPageChange={handlePageChange}
            onItemsPerPageChange={handleItemsPerPageChange}
            showItemsPerPage={true}
            itemsPerPageOptions={[5, 10, 25, 50]}
          />
        </div>
      )}
    </div>
  );
};