import React from 'react';

interface PaginationControlsProps {
  currentPage: number;
  totalPages: number;
  onPageChange: (page: number) => void;
}

const PaginationControls: React.FC<PaginationControlsProps> = ({
  currentPage,
  totalPages,
  onPageChange
}) => {
  return (
    <div className="pagination-controls">
      <button 
        className="pagination-button"
        onClick={() => onPageChange(currentPage - 1)} 
        disabled={currentPage === 1}
        aria-label="Previous page"
      >
        Previous
      </button>
      
      <span className="pagination-info">
        Page {currentPage} of {totalPages || 1}
      </span>
      
      <button 
        className="pagination-button"
        onClick={() => onPageChange(currentPage + 1)} 
        disabled={currentPage === totalPages || totalPages === 0}
        aria-label="Next page"
      >
        Next
      </button>
    </div>
  );
};

export default PaginationControls;