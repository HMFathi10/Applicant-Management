import { ApplicantStatus } from '../types';
import axios from 'axios';

const API_URL = 'http://localhost:5259/api';

// Backend Applicant interface (matches backend model)
interface BackendApplicant {
  id: number;
  name: string;
  familyName: string;
  emailAddress: string;
  phone: string;
  age: number;
  address: string;
  countryOfOrigin: string;
  appliedDate: string;
  hired: boolean;
  createdDate?: string;
  lastModifiedDate?: string;
}

// Frontend Applicant interface
interface FrontendApplicant {
  id: string;
  name: string;
  familyName: string;
  emailAddress: string;
  phone: string;
  age: number;
  address: string;
  countryOfOrigin: string;
  appliedDate: string;
  hired: boolean;
  createdAt: string;
  updatedAt: string;
}

// Paginated response interface (matches backend PaginatedResponseDto)
interface PaginatedResponse<T> {
  items: T[];
  totalCount: number;
  currentPage: number;
  pageSize: number;
  totalPages: number;
  hasPreviousPage: boolean;
  hasNextPage: boolean;
}

// Helper function to convert backend applicant to frontend format
const convertBackendToFrontend = (backendApplicant: BackendApplicant): FrontendApplicant => {
  return {
    id: backendApplicant.id.toString(),
    name: backendApplicant.name,
    familyName: backendApplicant.familyName,
    emailAddress: backendApplicant.emailAddress,
    phone: backendApplicant.phone,
    age: backendApplicant.age,
    address: backendApplicant.address,
    countryOfOrigin: backendApplicant.countryOfOrigin,
    appliedDate: backendApplicant.appliedDate,
    hired: backendApplicant.hired,
    createdAt: backendApplicant.createdDate || new Date().toISOString(),
    updatedAt: backendApplicant.lastModifiedDate || new Date().toISOString()
  };
};

// Helper function to convert frontend applicant to backend format
const convertFrontendToBackend = (frontendApplicant: Omit<FrontendApplicant, 'id' | 'createdAt' | 'updatedAt'>): Omit<BackendApplicant, 'id'> => {
  return {
    name: frontendApplicant.name,
    familyName: frontendApplicant.familyName,
    emailAddress: frontendApplicant.emailAddress,
    phone: frontendApplicant.phone,
    age: frontendApplicant.age,
    address: frontendApplicant.address,
    countryOfOrigin: frontendApplicant.countryOfOrigin,
    appliedDate: frontendApplicant.appliedDate,
    hired: frontendApplicant.hired
  };
};

export class ApplicantService {
  private fallbackApplicants: FrontendApplicant[] = [];

  async getApplicants(page: number = 1, pageSize: number = 10, searchTerm: string = ''): Promise<PaginatedResponse<FrontendApplicant>> {
    try {
      console.log('Fetching applicants with params:', { page, pageSize, searchTerm });
      const response = await axios.get<BackendApplicant[]>(`${API_URL}/applicants`, {
        params: { page, pageSize, searchTerm }
      });
      
      console.log('API Response:', response.data);
      
      // Backend returns array directly, not paginated response
      const backendApplicants = response.data || [];
      
      // Convert to frontend format
      const frontendApplicants = backendApplicants.map(convertBackendToFrontend);
      
      console.log('Converted applicants:', frontendApplicants);
      
      // Calculate pagination info based on the array length
      const totalCount = frontendApplicants.length;
      const totalPages = Math.ceil(totalCount / pageSize);
      const hasPreviousPage = page > 1;
      const hasNextPage = page < totalPages;
      
      // For now, return all items since backend doesn't support pagination
      // In a real implementation, the backend should return paginated data
      const result = {
        items: frontendApplicants,
        totalCount: totalCount,
        currentPage: page,
        pageSize: pageSize,
        totalPages: totalPages,
        hasPreviousPage: hasPreviousPage,
        hasNextPage: hasNextPage
      };
      
      console.log('Returning paginated result:', result);
      return result;
    } catch (error) {
      console.error('Error fetching applicants:', error);
      // Fallback to mock implementation with empty data
      return {
        items: [],
        totalCount: 0,
        currentPage: 1,
        pageSize: 10,
        totalPages: 1,
        hasPreviousPage: false,
        hasNextPage: false
      };
    }
  }

  async getApplicantById(id: string): Promise<FrontendApplicant | null> {
    try {
      const numericId = parseInt(id, 10);
      if (isNaN(numericId)) {
        throw new Error('Invalid applicant ID format');
      }
      const response = await axios.get<BackendApplicant>(`${API_URL}/applicants/${numericId}`);
      return convertBackendToFrontend(response.data);
    } catch (error) {
      console.error(`Error fetching applicant with id ${id}:`, error);
      return this.fallbackApplicants.find(applicant => applicant.id === id) || null;
    }
  }

  async createApplicant(applicantData: Omit<FrontendApplicant, 'id' | 'createdAt' | 'updatedAt'>): Promise<FrontendApplicant> {
    try {
      const backendData = convertFrontendToBackend(applicantData);
      const response = await axios.post<number>(`${API_URL}/applicants`, backendData);
      const newId = response.data;
      
      // Fetch the newly created applicant to get the complete data
      const newApplicantResponse = await axios.get<BackendApplicant>(`${API_URL}/applicants/${newId}`);
      return convertBackendToFrontend(newApplicantResponse.data);
    } catch (error) {
      console.error('Error creating applicant:', error);
      // Fallback to mock implementation
      const newApplicant: FrontendApplicant = {
        ...applicantData,
        id: Date.now().toString(),
        createdAt: new Date().toISOString(),
        updatedAt: new Date().toISOString()
      };
      this.fallbackApplicants.push(newApplicant);
      return newApplicant;
    }
  }

  async updateApplicant(id: string, updates: Partial<FrontendApplicant>): Promise<FrontendApplicant | null> {
    try {
      const numericId = parseInt(id, 10);
      if (isNaN(numericId)) {
        throw new Error('Invalid applicant ID format');
      }
      
      // Convert updates to backend format
      const backendUpdates = convertFrontendToBackend({
        name: updates.name || '',
        familyName: updates.familyName || '',
        emailAddress: updates.emailAddress || '',
        phone: updates.phone || '',
        age: updates.age || 0,
        address: updates.address || '',
        countryOfOrigin: updates.countryOfOrigin || '',
        appliedDate: updates.appliedDate || '',
        hired: updates.hired || false
      });

      await axios.put(`${API_URL}/applicants/${numericId}`, backendUpdates);
      
      // Fetch the updated applicant to get the complete data
      const updatedApplicantResponse = await axios.get<BackendApplicant>(`${API_URL}/applicants/${numericId}`);
      return convertBackendToFrontend(updatedApplicantResponse.data);
    } catch (error) {
      console.error(`Error updating applicant with id ${id}:`, error);
      // Fallback to mock implementation
      const index = this.fallbackApplicants.findIndex(applicant => applicant.id === id);
      if (index === -1) return null;

      this.fallbackApplicants[index] = {
        ...this.fallbackApplicants[index],
        ...updates,
        updatedAt: new Date().toISOString()
      };

      return this.fallbackApplicants[index];
    }
  }

  async deleteApplicant(id: string): Promise<boolean> {
    try {
      const numericId = parseInt(id, 10);
      if (isNaN(numericId)) {
        throw new Error('Invalid applicant ID format');
      }
      await axios.delete(`${API_URL}/applicants/${numericId}`);
      return true;
    } catch (error) {
      console.error(`Error deleting applicant with id ${id}:`, error);
      // Fallback to mock implementation
      const initialLength = this.fallbackApplicants.length;
      this.fallbackApplicants = this.fallbackApplicants.filter(applicant => applicant.id !== id);
      return this.fallbackApplicants.length < initialLength;
    }
  }

  async searchApplicants(query: string): Promise<FrontendApplicant[]> {
    try {
      const response = await axios.get<BackendApplicant[]>(`${API_URL}/applicants/search`, {
        params: { query }
      });
      return response.data.map(convertBackendToFrontend);
    } catch (error) {
      console.error('Error searching applicants:', error);
      // Fallback to mock implementation
      const lowercaseQuery = query.toLowerCase();
      return this.fallbackApplicants.filter(applicant =>
        applicant.name.toLowerCase().includes(lowercaseQuery)
      );
    }
  }

  /**
   * Search applicants with server-side pagination and filtering
   * This method is optimized for performance by using server-side pagination
   * and filtering, reducing the amount of data transferred over the network.
   */
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
      console.log('Service: searchApplicantsPaginated called with params:', params);
      
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

      console.log('Service: API response received:', response.data);

      // Convert backend response to frontend format
      const convertedItems = response.data.items.map(convertBackendToFrontend);
      console.log('Service: Converted items:', convertedItems);

      const result = {
        items: convertedItems,
        totalCount: response.data.totalCount,
        currentPage: response.data.currentPage,
        pageSize: response.data.pageSize,
        totalPages: response.data.totalPages,
        hasPreviousPage: response.data.hasPreviousPage,
        hasNextPage: response.data.hasNextPage
      };

      console.log('Service: Returning result:', result);
      return result;
    } catch (error) {
      console.error('Error searching applicants with pagination:', error);
      // Fallback to mock implementation
      const fallbackResults = params.searchTerm 
        ? this.fallbackApplicants.filter(applicant =>
            applicant.name.toLowerCase().includes(params.searchTerm!.toLowerCase())
          )
        : this.fallbackApplicants;

      const pageNumber = params.pageNumber || 1;
      const pageSize = params.pageSize || 10;
      const startIndex = (pageNumber - 1) * pageSize;
      const endIndex = startIndex + pageSize;
      const paginatedItems = fallbackResults.slice(startIndex, endIndex);

      return {
        items: paginatedItems,
        totalCount: fallbackResults.length,
        currentPage: pageNumber,
        pageSize: pageSize,
        totalPages: Math.ceil(fallbackResults.length / pageSize),
        hasPreviousPage: pageNumber > 1,
        hasNextPage: endIndex < fallbackResults.length
      };
    }
  }

  async filterApplicants(filters: {
    status?: ApplicantStatus[];
    department?: string[];
    experienceMin?: number;
    experienceMax?: number;
    location?: string[];
    ratingMin?: number;
  }): Promise<FrontendApplicant[]> {
    try {
      const response = await axios.get<BackendApplicant[]>(`${API_URL}/applicants/filter`, {
        params: filters
      });
      return response.data.map(convertBackendToFrontend);
    } catch (error) {
      console.error('Error filtering applicants:', error);
      // Fallback to mock implementation
      return [];
    }
  }
}

export const applicantService = new ApplicantService();