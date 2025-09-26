import { useState } from 'react';

interface PaginationOptions {
  initialPage?: number;
  initialPageSize?: number;
}

interface PaginationResult {
  page: number;
  pageSize: number;
  setPage: (page: number) => void;
  setPageSize: (size: number) => void;
  nextPage: () => void;
  prevPage: () => void;
  resetPagination: () => void;
}

/**
 * A custom hook for managing pagination state
 * 
 * @param options Pagination options
 * @returns Pagination state and methods
 */
function usePagination({ 
  initialPage = 1, 
  initialPageSize = 10 
}: PaginationOptions = {}): PaginationResult {
  const [page, setPage] = useState<number>(initialPage);
  const [pageSize, setPageSize] = useState<number>(initialPageSize);

  const nextPage = () => {
    setPage(prev => prev + 1);
  };

  const prevPage = () => {
    setPage(prev => prev > 1 ? prev - 1 : 1);
  };

  const resetPagination = () => {
    setPage(initialPage);
    setPageSize(initialPageSize);
  };

  return {
    page,
    pageSize,
    setPage,
    setPageSize,
    nextPage,
    prevPage,
    resetPagination
  };
}

export default usePagination;