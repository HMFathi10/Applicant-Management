export interface Applicant {
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

// Alias for frontend-specific applicant type
export type FrontendApplicant = Applicant;

// Paginated response type
export interface PaginatedResponse<T> {
  items: T[];
  applicants: T[];
  totalCount: number;
  pageSize: number;
  currentPage: number;
  totalPages: number;
}

export enum ApplicantStatus {
  NEW = 'new',
  SCREENING = 'screening',
  INTERVIEW = 'interview',
  OFFER = 'offer',
  HIRED = 'hired',
  REJECTED = 'rejected'
}

export enum WorkAuthorization {
  CITIZEN = 'citizen',
  PERMANENT_RESIDENT = 'permanent_resident',
  WORK_VISA = 'work_visa',
  SPONSORSHIP = 'sponsorship'
}

export interface ApplicantFilters {
  search?: string;
  status?: ApplicantStatus[];
  department?: string[];
  dateRange?: {
    start: string;
    end: string;
  };
}

export interface ApplicantSort {
  field: keyof Applicant;
  direction: 'asc' | 'desc';
}

export interface ValidationError {
  field: string;
  message: string;
}

export interface ApiResponse<T> {
  data: T;
  message?: string;
  success: boolean;
  errors?: ValidationError[];
}