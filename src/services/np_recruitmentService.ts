import api from './np_api';
import type { CourierApplicant, ApplicantFilter, PipelineSummary } from '@/types';

/**
 * @backend-needed GET /v1/np/applicants — Loc: build this endpoint
 * @backend-needed GET /v1/np/pipeline — Loc: build this endpoint
 */
export const recruitmentService = {
  /** @backend-needed GET /v1/np/applicants */
  async getApplicants(filters?: ApplicantFilter): Promise<CourierApplicant[]> {
    try {
      const params: Record<string, string> = {};
      if (filters?.stage) params.stage = filters.stage;
      if (filters?.search) params.search = filters.search;
      const { data } = await api.get<CourierApplicant[]>('/v1/np/applicants', { params });
      return data ?? [];
    } catch (e: any) {
      if (e?.response?.status === 404 || e?.code === 'ERR_NETWORK') {
        // FALLBACK: returns empty array until backend is implemented
        return [];
      }
      console.error('recruitmentService.getApplicants failed:', e);
      return [];
    }
  },

  /** @backend-needed GET /v1/np/applicants/{id} */
  async getApplicantById(id: number): Promise<CourierApplicant | undefined> {
    try {
      const { data } = await api.get<CourierApplicant>(`/v1/np/applicants/${id}`);
      return data;
    } catch (e) {
      console.error(`recruitmentService.getApplicantById(${id}) failed:`, e);
      return undefined;
    }
  },

  /** @backend-needed GET /v1/np/pipeline */
  async getPipelineSummary(): Promise<PipelineSummary[]> {
    try {
      const { data } = await api.get<PipelineSummary[]>('/v1/np/pipeline');
      return data ?? [];
    } catch (e: any) {
      if (e?.response?.status === 404 || e?.code === 'ERR_NETWORK') {
        // FALLBACK: returns empty array until backend is implemented
        return [];
      }
      console.error('recruitmentService.getPipelineSummary failed:', e);
      return [];
    }
  },

  /** @backend-needed POST /v1/np/applicants */
  async createApplicant(data: Partial<CourierApplicant>): Promise<CourierApplicant> {
    const { data: result } = await api.post<CourierApplicant>('/v1/np/applicants', data);
    return result;
  },

  /** @backend-needed POST /v1/np/applicants/{id}/advance — Loc: advance to next stage */
  async advanceStage(id: number): Promise<void> {
    try {
      await api.post(`/v1/np/applicants/${id}/advance`, {});
    } catch (e) {
      console.error(`recruitmentService.advanceStage(${id}) failed:`, e);
    }
  },

  /** @backend-needed POST /v1/np/applicants/{id}/reject — Loc: reject applicant */
  async rejectApplicant(id: number, reason: string): Promise<void> {
    try {
      await api.post(`/v1/np/applicants/${id}/reject`, { reason });
    } catch (e) {
      console.error(`recruitmentService.rejectApplicant(${id}) failed:`, e);
    }
  },

  /** @backend-needed POST /v1/np/applicants/{id}/approve — Loc: approve applicant */
  async approveApplicant(id: number): Promise<void> {
    try {
      await api.post(`/v1/np/applicants/${id}/approve`, {});
    } catch (e) {
      console.error(`recruitmentService.approveApplicant(${id}) failed:`, e);
    }
  },
};
