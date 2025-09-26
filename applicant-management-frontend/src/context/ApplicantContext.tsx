import React, { createContext, useContext, useReducer } from 'react';
import type { ReactNode } from 'react';
import type { Applicant } from '../types/applicant';
import { applicantService } from '../services/applicantService';

const API_URL = 'http://localhost:5259/api';
// State interface
interface ApplicantState {
  applicants: Applicant[];
  loading: boolean;
  error: string | null;
  searchTerm: string;
  departmentFilter: string;
  sortBy: string;
  sortOrder: 'asc' | 'desc';
  currentPage: number;
  itemsPerPage: number;
  totalItems: number;
  totalPages: number;
}

// Action types
type ApplicantAction =
  | { type: 'SET_LOADING'; payload: boolean }
  | { type: 'SET_ERROR'; payload: string | null }
  | { type: 'SET_APPLICANTS'; payload: Applicant[] }
  | { type: 'ADD_APPLICANT'; payload: Applicant }
  | { type: 'UPDATE_APPLICANT'; payload: Applicant }
  | { type: 'DELETE_APPLICANT'; payload: string }
  | { type: 'SET_SEARCH_TERM'; payload: string }
  | { type: 'SET_DEPARTMENT_FILTER'; payload: string }
  | { type: 'SET_SORT'; payload: { by: string; order: 'asc' | 'desc' } }
  | { type: 'RESET_FILTERS' }
  | { type: 'SET_SEARCH_RESULTS'; payload: Applicant[] }
  | { type: 'SET_FILTERED_RESULTS'; payload: Applicant[] }
  | { type: 'SET_CURRENT_PAGE'; payload: number }
  | { type: 'SET_ITEMS_PER_PAGE'; payload: number }
  | { type: 'SET_TOTAL_ITEMS'; payload: number }
  | { type: 'SET_PAGINATED_RESULTS'; payload: { items: Applicant[]; totalCount: number; currentPage: number; pageSize: number; totalPages: number; hasPreviousPage: boolean; hasNextPage: boolean } };

// Initial state
const initialState: ApplicantState = {
  applicants: [],
  loading: false,
  error: null,
  searchTerm: '',
  departmentFilter: 'all',
  sortBy: 'name',
  sortOrder: 'asc',
  currentPage: 1,
  itemsPerPage: 10,
  totalItems: 0,
  totalPages: 1,
};

// Reducer function
const applicantReducer = (state: ApplicantState, action: ApplicantAction): ApplicantState => {
  switch (action.type) {
    case 'SET_LOADING':
      return { ...state, loading: action.payload };
    case 'SET_ERROR':
      return { ...state, error: action.payload };
    case 'SET_APPLICANTS':
      return { ...state, applicants: action.payload };
    case 'ADD_APPLICANT':
      return { ...state, applicants: [...state.applicants, action.payload] };
    case 'UPDATE_APPLICANT':
      return {
        ...state,
        applicants: state.applicants.map((applicant) =>
          applicant.id === action.payload.id ? action.payload : applicant
        ),
      };
    case 'DELETE_APPLICANT':
      return {
        ...state,
        applicants: state.applicants.filter((applicant) => applicant.id !== action.payload),
      };
    case 'SET_SEARCH_TERM':
      return { ...state, searchTerm: action.payload };
    case 'SET_DEPARTMENT_FILTER':
      return { ...state, departmentFilter: action.payload };
    case 'SET_SORT':
      return { ...state, sortBy: action.payload.by, sortOrder: action.payload.order };
    case 'RESET_FILTERS':
      return {
        ...state,
        searchTerm: '',
        departmentFilter: 'all',
        sortBy: 'name',
        sortOrder: 'asc',
      };
    case 'SET_SEARCH_RESULTS':
      return { ...state, applicants: action.payload };
    case 'SET_FILTERED_RESULTS':
      return { ...state, applicants: action.payload };
    case 'SET_CURRENT_PAGE':
      return { ...state, currentPage: action.payload };
    case 'SET_ITEMS_PER_PAGE':
      return { ...state, itemsPerPage: action.payload };
    case 'SET_TOTAL_ITEMS':
      return { ...state, totalItems: action.payload };
    case 'SET_PAGINATED_RESULTS':
      console.log('Reducer: SET_PAGINATED_RESULTS received:', action.payload);
      const newState = { 
        ...state, 
        applicants: action.payload.items,
        totalItems: action.payload.totalCount,
        currentPage: action.payload.currentPage,
        itemsPerPage: action.payload.pageSize,
        totalPages: action.payload.totalPages
      };
      console.log('Reducer: New state after SET_PAGINATED_RESULTS:', newState);
      return newState;
    default:
      return state;
  }
};

// Context
export interface ApplicantContextType {
  state: ApplicantState;
  dispatch: React.Dispatch<ApplicantAction>;
  actions: {
    addApplicant: (applicant: Omit<Applicant, 'id' | 'createdAt' | 'updatedAt'>) => Promise<any>;
    updateApplicant: (applicant: Applicant) => Promise<any>;
    deleteApplicant: (id: string) => Promise<void>;
    searchApplicants: (query: string) => Promise<void>;
    filterApplicants: (filters: { experience?: number; location?: string }) => Promise<void>;
    fetchPaginatedApplicants: (page: number, pageSize: number, searchTerm: string) => Promise<any>;
    setSearchTerm: (term: string) => void;
    setDepartmentFilter: (department: string) => void;
    setSort: (by: string, order: 'asc' | 'desc') => void;
    resetFilters: () => void;
    setCurrentPage: (page: number) => void;
    setItemsPerPage: (itemsPerPage: number) => void;
    setTotalItems: (totalItems: number) => void;
  };
}

const ApplicantContext = createContext<ApplicantContextType | undefined>(undefined);

// Provider props
interface ApplicantProviderProps {
  children: ReactNode;
}

// Provider component
export const ApplicantProvider: React.FC<ApplicantProviderProps> = ({ children }) => {
  const [state, dispatch] = useReducer(applicantReducer, initialState);

  // Data is now managed by the backend API, no localStorage needed
  // Removed loadApplicants to prevent redundant API calls
  // Only fetchPaginatedApplicants will be used for data fetching

  const addApplicant = async (applicantData: Omit<Applicant, 'id' | 'createdAt' | 'updatedAt'>) => {
    try {
      dispatch({ type: 'SET_LOADING', payload: true });
      dispatch({ type: 'SET_ERROR', payload: null });

      // Create applicant via API
      const response = await fetch(`${API_URL}/applicants`, {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
        },
        body: JSON.stringify(applicantData),
      });

      if (!response.ok) {
        // Try to get detailed error message from response
        let errorMessage = 'Failed to create applicant';
        try {
          const errorData = await response.json();
          errorMessage = errorData.message || errorData.title || `HTTP ${response.status}: ${response.statusText}`;
        } catch {
          errorMessage = `HTTP ${response.status}: ${response.statusText}`;
        }
        throw new Error(errorMessage);
      }

      const newId = await response.json();
      
      // Fetch the newly created applicant
      const getResponse = await fetch(`${API_URL}/applicants/${newId}`);
      if (!getResponse.ok) {
        throw new Error('Failed to fetch created applicant');
      }
      const newApplicant = await getResponse.json();

      dispatch({ type: 'ADD_APPLICANT', payload: newApplicant });
      return newApplicant;
    } catch (error) {
      console.error('Error adding applicant:', error);
      const errorMessage = error instanceof Error ? error.message : 'Failed to add applicant';
      dispatch({ type: 'SET_ERROR', payload: errorMessage });
      throw error;
    } finally {
      dispatch({ type: 'SET_LOADING', payload: false });
    }
  };

  const updateApplicant = async (applicant: Applicant) => {
    try {
      dispatch({ type: 'SET_LOADING', payload: true });
      dispatch({ type: 'SET_ERROR', payload: null });

      // Update applicant via API
      const numericId = parseInt(applicant.id, 10);
      if (isNaN(numericId)) {
        throw new Error('Invalid applicant ID format');
      }

      // First, get the existing applicant to retrieve the current RowVersion
      const getExistingResponse = await fetch(`${API_URL}/applicants/${numericId}`);
      if (!getExistingResponse.ok) {
        throw new Error('Failed to fetch existing applicant data');
      }
      const existingApplicant = await getExistingResponse.json();
      
      if (!existingApplicant.rowVersion) {
        throw new Error('RowVersion is required but not found in existing applicant data');
      }

      // Convert phone number to digits only format (remove + and spaces)
      const cleanPhone = applicant.phone ? applicant.phone.replace(/[\+\s]/g, '') : '';

      // Convert applicant data to backend format (ensure proper field names)
      const backendData = {
        id: numericId,
        name: applicant.name || '',
        familyName: applicant.familyName || '',
        emailAddress: applicant.emailAddress || '',
        phone: cleanPhone,
        age: applicant.age || 0,
        address: applicant.address || '',
        countryOfOrigin: applicant.countryOfOrigin || '',
        appliedDate: applicant.appliedDate || new Date().toISOString(),
        hired: applicant.hired || false,
        rowVersion: existingApplicant.rowVersion
      };

      const response = await fetch(`${API_URL}/applicants/${numericId}`, {
        method: 'PUT',
        headers: {
          'Content-Type': 'application/json',
        },
        body: JSON.stringify(backendData),
      });

      if (!response.ok) {
        // Try to get detailed error message from response
        let errorMessage = 'Failed to update applicant';
        try {
          const errorData = await response.json();
          errorMessage = errorData.message || errorData.title || `HTTP ${response.status}: ${response.statusText}`;
        } catch {
          errorMessage = `HTTP ${response.status}: ${response.statusText}`;
        }
        throw new Error(errorMessage);
      }

      // Fetch the updated applicant
      const getResponse = await fetch(`${API_URL}/applicants/${numericId}`);
      if (!getResponse.ok) {
        throw new Error('Failed to fetch updated applicant');
      }
      const updatedApplicant = await getResponse.json();

      dispatch({ type: 'UPDATE_APPLICANT', payload: updatedApplicant });
      return updatedApplicant;
    } catch (error) {
      console.error('Error updating applicant:', error);
      const errorMessage = error instanceof Error ? error.message : 'Failed to update applicant';
      dispatch({ type: 'SET_ERROR', payload: errorMessage });
      throw error;
    } finally {
      dispatch({ type: 'SET_LOADING', payload: false });
    }
  };

  const deleteApplicant = async (id: string) => {
    try {
      dispatch({ type: 'SET_LOADING', payload: true });
      dispatch({ type: 'SET_ERROR', payload: null });

      // Delete applicant via API
      const numericId = parseInt(id, 10);
      if (isNaN(numericId)) {
        throw new Error('Invalid applicant ID format');
      }

      // First, get the existing applicant to retrieve the current RowVersion
      const getExistingResponse = await fetch(`${API_URL}/applicants/${numericId}`);
      if (!getExistingResponse.ok) {
        throw new Error('Failed to fetch existing applicant data');
      }
      const existingApplicant = await getExistingResponse.json();
      
      if (!existingApplicant.rowVersion) {
        throw new Error('RowVersion is required but not found in existing applicant data');
      }

      // Convert rowVersion to query parameter format
      const rowVersionParam = encodeURIComponent(existingApplicant.rowVersion);

      const response = await fetch(`${API_URL}/applicants/${numericId}?rowVersion=${rowVersionParam}`, {
        method: 'DELETE',
        headers: {
          'Content-Type': 'application/json',
        },
      });

      if (!response.ok) {
        // Try to get detailed error message from response
        let errorMessage = 'Failed to delete applicant';
        try {
          const errorData = await response.json();
          errorMessage = errorData.message || errorData.title || `HTTP ${response.status}: ${response.statusText}`;
        } catch {
          errorMessage = `HTTP ${response.status}: ${response.statusText}`;
        }
        throw new Error(errorMessage);
      }

      dispatch({ type: 'DELETE_APPLICANT', payload: id });
    } catch (error) {
      console.error('Error deleting applicant:', error);
      const errorMessage = error instanceof Error ? error.message : 'Failed to delete applicant';
      dispatch({ type: 'SET_ERROR', payload: errorMessage });
      throw error;
    } finally {
      dispatch({ type: 'SET_LOADING', payload: false });
    }
  };

  const searchApplicants = async (query: string) => {
    try {
      dispatch({ type: 'SET_LOADING', payload: true });
      dispatch({ type: 'SET_ERROR', payload: null });

      // Search applicants via API
      const response = await fetch(`${API_URL}/applicants/search?query=${encodeURIComponent(query)}`);
      
      if (!response.ok) {
        throw new Error('Failed to search applicants');
      }

      const searchResults = await response.json();
      dispatch({ type: 'SET_SEARCH_RESULTS', payload: searchResults });
    } catch (error) {
      console.error('Error searching applicants:', error);
      dispatch({ type: 'SET_ERROR', payload: 'Failed to search applicants' });
      throw error;
    } finally {
      dispatch({ type: 'SET_LOADING', payload: false });
    }
  };

  const filterApplicants = async (filters: { experience?: number; location?: string }) => {
    try {
      dispatch({ type: 'SET_LOADING', payload: true });
      dispatch({ type: 'SET_ERROR', payload: null });

      // Build query parameters
      const params = new URLSearchParams();
      if (filters.experience !== undefined) {
        params.append('experience', filters.experience.toString());
      }
      if (filters.location) {
        params.append('location', filters.location);
      }

      // Filter applicants via API
      const response = await fetch(`${API_URL}/applicants/filter?${params.toString()}`);
      
      if (!response.ok) {
        throw new Error('Failed to filter applicants');
      }

      const filteredResults = await response.json();
      dispatch({ type: 'SET_FILTERED_RESULTS', payload: filteredResults });
    } catch (error) {
      console.error('Error filtering applicants:', error);
      dispatch({ type: 'SET_ERROR', payload: 'Failed to filter applicants' });
      throw error;
    } finally {
      dispatch({ type: 'SET_LOADING', payload: false });
    }
  };

  const setSearchTerm = (term: string) => {
    dispatch({ type: 'SET_SEARCH_TERM', payload: term });
  };

  const setDepartmentFilter = (department: string) => {
    dispatch({ type: 'SET_DEPARTMENT_FILTER', payload: department });
  };

  const setSort = (by: string, order: 'asc' | 'desc') => {
    dispatch({ type: 'SET_SORT', payload: { by, order } });
  };

  const resetFilters = () => {
    dispatch({ type: 'RESET_FILTERS' });
  };

  const setCurrentPage = (page: number) => {
    dispatch({ type: 'SET_CURRENT_PAGE', payload: page });
  };

  const setItemsPerPage = (itemsPerPage: number) => {
    dispatch({ type: 'SET_ITEMS_PER_PAGE', payload: itemsPerPage });
  };

  const setTotalItems = (totalItems: number) => {
    dispatch({ type: 'SET_TOTAL_ITEMS', payload: totalItems });
  };

  // Fetch applicants with pagination and search
  const fetchPaginatedApplicants = async (page: number, pageSize: number, searchTerm: string) => {
    try {
      dispatch({ type: 'SET_LOADING', payload: true });
      dispatch({ type: 'SET_ERROR', payload: null });
      
      console.log('Context: Fetching paginated applicants with params:', { page, pageSize, searchTerm });
      const result = await applicantService.searchApplicantsPaginated({
        searchTerm: searchTerm,
        pageNumber: page,
        pageSize: pageSize
      });
      
      console.log('Context: Received result from service:', result);
      
      dispatch({ 
        type: 'SET_PAGINATED_RESULTS', 
        payload: {
          items: result.items || [],
          totalCount: result.totalCount,
          currentPage: result.currentPage,
          pageSize: result.pageSize,
          totalPages: result.totalPages,
          hasPreviousPage: result.currentPage > 1,
          hasNextPage: result.currentPage < result.totalPages
        }
      });
      
      return result;
    } catch (error) {
      console.error('Error fetching paginated applicants:', error);
      dispatch({ type: 'SET_ERROR', payload: 'Failed to fetch applicants' });
      throw error;
    } finally {
      dispatch({ type: 'SET_LOADING', payload: false });
    }
  };

  const actions = {
    addApplicant,
    updateApplicant,
    deleteApplicant,
    searchApplicants,
    filterApplicants,
    fetchPaginatedApplicants,
    setSearchTerm,
    setDepartmentFilter,
    setSort,
    resetFilters,
    setCurrentPage,
    setItemsPerPage,
    setTotalItems,
  };

  return (
    <ApplicantContext.Provider value={{ state, dispatch, actions }}>
      {children}
    </ApplicantContext.Provider>
  );
};

// Custom hook to use the applicant context
export const useApplicantContext = () => {
  const context = useContext(ApplicantContext);
  if (context === undefined) {
    throw new Error('useApplicantContext must be used within an ApplicantProvider');
  }
  return context;
};

export default ApplicantContext;