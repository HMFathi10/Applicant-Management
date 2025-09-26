# Server-Side Pagination with Debounced Search Implementation

## Problem Statement
The current implementation of the applicant management system loads all applicants at once, leading to performance issues as the dataset grows. Additionally, the search functionality triggers an API call on every keystroke, causing unnecessary server load and potential performance degradation. This implementation aims to solve these issues by implementing server-side pagination and debounced search functionality.

## Proposed Solution
The solution implements server-side pagination with a debounced search function to optimize data retrieval and reduce unnecessary API calls. This approach ensures a smooth user experience even with large datasets.

## Server-Side Implementation (ASP.NET Core)

### 1. API Endpoint Design
The backend already has an endpoint for fetching paginated data with search functionality:
```csharp
[HttpGet("advanced-search")]
public async Task<ActionResult<List<Applicant>>> AdvancedSearch(
    [FromQuery] string searchTerm = null,
    [FromQuery] int? minAge = null,
    [FromQuery] int? maxAge = null,
    [FromQuery] string countryOfOrigin = null,
    [FromQuery] bool? isHired = null,
    [FromQuery] DateTime? appliedDateFrom = null,
    [FromQuery] DateTime? appliedDateTo = null,
    [FromQuery] string sortBy = "Name",
    [FromQuery] bool sortDescending = false,
    [FromQuery] int pageNumber = 1,
    [FromQuery] int pageSize = 50,
    [FromQuery] bool includeDeleted = false)
```

### 2. Pagination Logic
The server-side pagination logic is implemented in the `GetApplicantsWithFiltersHandler` class, which calculates the total number of pages and retrieves the correct subset of data:
```csharp
// Return paginated response
return new PaginatedResponseDto<Applicant>
{
    Items = applicantList,
    TotalCount = totalCount,
    CurrentPage = request.PageNumber,
    PageSize = request.PageSize,
    TotalPages = (int)Math.Ceiling((double)totalCount / request.PageSize),
    HasPreviousPage = request.PageNumber > 1,
    HasNextPage = request.PageNumber < (int)Math.Ceiling((double)totalCount / request.PageSize)
};
```

### 3. Database Query Optimization
The database query is optimized in the `ApplicantRepository` class:
```csharp
// Apply pagination
var skip = (page - 1) * pageSize;
query = query.Skip(skip).Take(pageSize);
```

## Client-Side Implementation (React.js)

### 1. Custom Hooks
Two custom hooks were created to handle debounced search and pagination:

#### useDebounce.ts
```typescript
import { useState, useEffect } from 'react';

function useDebounce<T>(value: T, delay: number = 500): T {
  const [debouncedValue, setDebouncedValue] = useState<T>(value);

  useEffect(() => {
    const timer = setTimeout(() => {
      setDebouncedValue(value);
    }, delay);

    return () => {
      clearTimeout(timer);
    };
  }, [value, delay]);

  return debouncedValue;
}

export default useDebounce;
```

#### usePagination.ts
```typescript
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
```

### 2. API Service
The `applicantService.ts` file was updated to optimize the `searchApplicantsPaginated` method:
```typescript
async searchApplicantsPaginated(params: {
  searchTerm?: string;
  pageNumber?: number;
  pageSize?: number;
  sortBy?: string;
  sortDescending?: boolean;
  minAge?: number;
  maxAge?: number;
  countryOfOrigin?: string;
  isHired?: boolean;
}): Promise<PaginatedResponse<FrontendApplicant>> {
  try {
    // Use server-side pagination and filtering
    const response = await axios.get<PaginatedResponse<BackendApplicant>>(`${API_URL}/applicants/advanced-search`, {
      params: {
        searchTerm: params.searchTerm || '',
        pageNumber: params.pageNumber || 1,
        pageSize: params.pageSize || 10,
        sortBy: params.sortBy || 'Name',
        sortDescending: params.sortDescending || false,
        minAge: params.minAge,
        maxAge: params.maxAge,
        countryOfOrigin: params.countryOfOrigin,
        isHired: params.isHired
      }
    });

    // Convert backend response to frontend format
    return {
      items: response.data.items.map(convertBackendToFrontend),
      totalCount: response.data.totalCount,
      currentPage: response.data.currentPage,
      pageSize: response.data.pageSize,
      totalPages: response.data.totalPages,
      hasPreviousPage: response.data.hasPreviousPage,
      hasNextPage: response.data.hasNextPage
    };
  } catch (error) {
    // Fallback implementation...
  }
}
```

### 3. React Component
A new component `ApplicantSearchTable.tsx` was created to demonstrate the server-side pagination with debounced search:
```typescript
// Key implementation details
const [searchTerm, setSearchTerm] = useState<string>('');
const debouncedSearchTerm = useDebounce(searchTerm, 500); // 500ms debounce delay

// State for pagination
const { page, pageSize, setPage, nextPage, prevPage } = usePagination();

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
```

## Performance Analysis

### Benefits of Server-Side Pagination
1. **Reduced Data Transfer**: Only the data needed for the current page is transferred over the network, reducing bandwidth usage.
2. **Improved Load Times**: The application loads faster since it doesn't need to process large datasets.
3. **Scalability**: The solution scales well with growing datasets since the server only processes a subset of data at a time.

### Benefits of Debounced Search
1. **Reduced API Calls**: The debounced search waits until the user stops typing for 500ms before making an API call, significantly reducing the number of requests.
2. **Improved User Experience**: The UI remains responsive during typing, and search results are displayed only when the user has finished typing.
3. **Reduced Server Load**: Fewer API calls mean less load on the server, improving overall system performance.

## Conclusion
The implementation of server-side pagination with debounced search has significantly improved the performance and user experience of the applicant management system. By reducing unnecessary API calls and optimizing data retrieval, the application can now handle large datasets efficiently while providing a smooth user experience.

## Future Improvements
1. **Caching**: Implement client-side caching to further reduce API calls for previously fetched pages.
2. **Prefetching**: Implement prefetching of adjacent pages to improve perceived performance.
3. **Advanced Filtering**: Add more advanced filtering options to further optimize data retrieval.