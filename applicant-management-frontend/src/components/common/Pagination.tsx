import React from 'react';
import { ChevronLeft, ChevronRight } from 'lucide-react';
import { Button } from './Button';

export interface PaginationProps {
  currentPage: number;
  totalPages: number;
  itemsPerPage: number;
  totalItems: number;
  onPageChange: (page: number) => void;
  onItemsPerPageChange?: (itemsPerPage: number) => void;
  showItemsPerPage?: boolean;
  itemsPerPageOptions?: number[];
}

interface PaginationComponentProps extends PaginationProps {}

export const Pagination: React.FC<PaginationComponentProps> = ({
  currentPage,
  totalPages,
  itemsPerPage,
  totalItems,
  onPageChange,
  onItemsPerPageChange,
  showItemsPerPage = true,
  itemsPerPageOptions = [5, 10, 25, 50]
}) => {
  const startItem = (currentPage - 1) * itemsPerPage + 1;
  const endItem = Math.min(currentPage * itemsPerPage, totalItems);

  const getPageNumbers = () => {
    const pages: (number | string)[] = [];
    const maxVisiblePages = 5;

    if (totalPages <= maxVisiblePages) {
      for (let i = 1; i <= totalPages; i++) {
        pages.push(i);
      }
    } else {
      pages.push(1);

      if (currentPage > 3) {
        pages.push('...');
      }

      const startPage = Math.max(2, currentPage - 1);
      const endPage = Math.min(totalPages - 1, currentPage + 1);

      for (let i = startPage; i <= endPage; i++) {
        pages.push(i);
      }

      if (currentPage < totalPages - 2) {
        pages.push('...');
      }

      pages.push(totalPages);
    }

    return pages;
  };

  const handlePageClick = (page: number) => {
    if (page >= 1 && page <= totalPages && page !== currentPage) {
      onPageChange(page);
    }
  };

  const handleItemsPerPageChange = (newItemsPerPage: number) => {
    if (onItemsPerPageChange) {
      onItemsPerPageChange(newItemsPerPage);
    }
  };

  return (
    <div style={{
      display: 'flex',
      justifyContent: 'space-between',
      alignItems: 'center',
      padding: '1rem',
      borderTop: '1px solid var(--color-border)',
      backgroundColor: 'var(--color-surface)'
    }}>
      {/* Items per page selector */}
      {showItemsPerPage && (
        <div style={{ display: 'flex', alignItems: 'center', gap: '0.5rem' }}>
          <span style={{ color: 'var(--color-text-secondary)', fontSize: '0.9rem' }}>
            Show:
          </span>
          <select
            value={itemsPerPage}
            onChange={(e) => handleItemsPerPageChange(parseInt(e.target.value, 10))}
            style={{
              padding: '0.25rem 0.5rem',
              border: '1px solid var(--color-border)',
              borderRadius: 'var(--border-radius-sm)',
              backgroundColor: 'var(--color-background)',
              color: 'var(--color-text)',
              fontSize: '0.9rem',
              cursor: 'pointer'
            }}
          >
            {itemsPerPageOptions.map(option => (
              <option key={option} value={option}>
                {option}
              </option>
            ))}
          </select>
          <span style={{ color: 'var(--color-text-secondary)', fontSize: '0.9rem' }}>
            items per page
          </span>
        </div>
      )}

      {/* Page info */}
      <div style={{ color: 'var(--color-text-secondary)', fontSize: '0.9rem' }}>
        Showing {startItem} to {endItem} of {totalItems} items
      </div>

      {/* Pagination controls */}
      <div style={{ display: 'flex', alignItems: 'center', gap: '0.25rem' }}>
        {/* Previous button */}
        <Button
          variant="outline"
          size="sm"
          onClick={() => handlePageClick(currentPage - 1)}
          disabled={currentPage === 1}
          style={{ padding: '0.25rem 0.5rem' }}
        >
          <ChevronLeft size={16} />
        </Button>

        {/* Page numbers */}
        {getPageNumbers().map((page, index) => (
          <React.Fragment key={index}>
            {page === '...' ? (
              <span style={{
                padding: '0.25rem 0.5rem',
                color: 'var(--color-text-secondary)',
                fontSize: '0.9rem'
              }}>
                ...
              </span>
            ) : (
              <Button
                variant={page === currentPage ? 'primary' : 'outline'}
                size="sm"
                onClick={() => handlePageClick(page as number)}
                style={{ 
                  padding: '0.25rem 0.5rem',
                  minWidth: '2rem',
                  fontSize: '0.9rem'
                }}
              >
                {page}
              </Button>
            )}
          </React.Fragment>
        ))}

        {/* Next button */}
        <Button
          variant="outline"
          size="sm"
          onClick={() => handlePageClick(currentPage + 1)}
          disabled={currentPage === totalPages}
          style={{ padding: '0.25rem 0.5rem' }}
        >
          <ChevronRight size={16} />
        </Button>
      </div>
    </div>
  );
};