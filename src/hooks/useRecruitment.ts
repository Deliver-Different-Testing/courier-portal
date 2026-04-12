import { useState, useEffect, useCallback } from 'react';
import { recruitmentService } from '@/services/np_recruitmentService';
import type { CourierApplicant, ApplicantFilter, PipelineSummary } from '@/types';

export function useRecruitment() {
  const [applicants, setApplicants] = useState<CourierApplicant[]>([]);
  const [pipelineSummary, setPipelineSummary] = useState<PipelineSummary[]>([]);
  const [filters, setFilters] = useState<ApplicantFilter>({});
  const [refreshKey, setRefreshKey] = useState(0);

  useEffect(() => {
    recruitmentService.getApplicants(filters).then(setApplicants).catch(() => setApplicants([]));
  }, [filters, refreshKey]);

  useEffect(() => {
    recruitmentService.getPipelineSummary().then(setPipelineSummary).catch(() => setPipelineSummary([]));
  }, [refreshKey]);

  const refresh = useCallback(() => setRefreshKey(k => k + 1), []);

  // Stub mutations — these endpoints don't exist on the backend yet
  const advanceStage = useCallback(async (id: number) => {
    // @backend-needed POST /v1/np/applicants/{id}/advance
    console.log('advanceStage stub:', id);
    refresh();
  }, [refresh]);

  const rejectApplicant = useCallback(async (id: number, _reason: string) => {
    // @backend-needed POST /v1/np/applicants/{id}/reject
    console.log('rejectApplicant stub:', id);
    refresh();
  }, [refresh]);

  const approveApplicant = useCallback(async (id: number) => {
    // @backend-needed POST /v1/np/applicants/{id}/approve
    console.log('approveApplicant stub:', id);
    refresh();
  }, [refresh]);

  return {
    applicants,
    pipelineSummary,
    filters,
    setFilters,
    advanceStage,
    rejectApplicant,
    approveApplicant,
    refresh,
  };
}

export function useApplicant(id: number) {
  const [applicant, setApplicant] = useState<CourierApplicant | null>(null);
  const [refreshKey, setRefreshKey] = useState(0);
  const refresh = useCallback(() => setRefreshKey(k => k + 1), []);

  useEffect(() => {
    recruitmentService.getApplicantById(id).then(a => setApplicant(a ?? null)).catch(() => setApplicant(null));
  }, [id, refreshKey]);

  return { applicant, refresh };
}
