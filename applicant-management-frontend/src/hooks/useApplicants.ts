import { useState, useEffect, useCallback } from 'react';
import { applicantService } from '../services';
import type { Applicant } from '../types'
export interface UseApplicantsReturn {
  applicants: Applicant[];
  loading: boolean;
  error: string | null;
  searchApplicants: (query: string) => Promise<void>;
  createApplicant: (applicant: Omit<Applicant, 'id' | 'createdAt' | 'updatedAt'>) => Promise<void>;
  updateApplicant: (id: string, updates: Partial<Applicant>) => Promise<void>;
  deleteApplicant: (id: string) => Promise<void>;
  refreshApplicants: () => Promise<void>;
}

export const useApplicants = (): UseApplicantsReturn => {
  const [applicants, setApplicants] = useState<Applicant[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  const refreshApplicants = useCallback(async () => {
    try {
      setLoading(true);
      setError(null);
      const data = await applicantService.getApplicants();
      setApplicants(data.items || []);
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Failed to load applicants');
    } finally {
      setLoading(false);
    }
  }, []);

  const searchApplicants = useCallback(async (query: string) => {
    try {
      setLoading(true);
      setError(null);
      if (query.trim() === '') {
        await refreshApplicants();
      } else {
        const results = await applicantService.searchApplicants(query);
        setApplicants(results);
      }
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Search failed');
    } finally {
      setLoading(false);
    }
  }, [refreshApplicants]);

  const createApplicant = useCallback(async (applicantData: Omit<Applicant, 'id' | 'createdAt' | 'updatedAt'>) => {
    try {
      setLoading(true);
      setError(null);
      const newApplicant = await applicantService.createApplicant(applicantData);
      setApplicants(prev => [newApplicant, ...prev]);
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Failed to create applicant');
      throw err;
    } finally {
      setLoading(false);
    }
  }, []);

  const updateApplicant = useCallback(async (id: string, updates: Partial<Applicant>) => {
    try {
      setLoading(true);
      setError(null);
      const updated = await applicantService.updateApplicant(id, updates);
      if (updated) {
        setApplicants(prev => prev.map(app => app.id === id ? updated : app));
      }
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Failed to update applicant');
      throw err;
    } finally {
      setLoading(false);
    }
  }, []);

  const deleteApplicant = useCallback(async (id: string) => {
    try {
      setLoading(true);
      setError(null);
      const success = await applicantService.deleteApplicant(id);
      if (success) {
        setApplicants(prev => prev.filter(app => app.id !== id));
      }
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Failed to delete applicant');
      throw err;
    } finally {
      setLoading(false);
    }
  }, []);

  useEffect(() => {
    refreshApplicants();
  }, [refreshApplicants]);

  return {
    applicants,
    loading,
    error,
    searchApplicants,
    createApplicant,
    updateApplicant,
    deleteApplicant,
    refreshApplicants
  };
};

export const useApplicant = (id: string | undefined) => {
  const [applicant, setApplicant] = useState<Applicant | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  const fetchApplicant = useCallback(async () => {
    if (!id) {
      setApplicant(null);
      setLoading(false);
      return;
    }

    try {
      setLoading(true);
      setError(null);
      const data = await applicantService.getApplicantById(id);
      setApplicant(data);
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Failed to load applicant');
      setApplicant(null);
    } finally {
      setLoading(false);
    }
  }, [id]);

  useEffect(() => {
    fetchApplicant();
  }, [fetchApplicant]);

  return { applicant, loading, error, refetch: fetchApplicant };
};