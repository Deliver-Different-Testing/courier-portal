import api from './np_api';
import type { CourierApplicant, ApplicantFilter, PipelineSummary } from '@/types';

export const recruitmentService = {
  async getApplicants(filters?: ApplicantFilter): Promise<CourierApplicant[]> {
    try {
      const params: Record<string, string> = {};
      if (filters?.stage) params.stage = filters.stage;
      if (filters?.search) params.search = filters.search;
      const { data } = await api.get<CourierApplicant[]>('/applicants', { params });
      return data;
    } catch (e) {
      console.error('recruitmentService.getApplicants failed:', e);
      return [];
    }
  },

  async getApplicantById(id: number): Promise<CourierApplicant | undefined> {
    try {
      const { data } = await api.get<CourierApplicant>(`/applicants/${id}`);
      return data;
    } catch (e) {
      console.error('recruitmentService.getApplicantById failed:', e);
      return undefined;
    }
  },

  async getPipelineSummary(): Promise<PipelineSummary[]> {
    try {
      const { data } = await api.get<PipelineSummary[]>('/applicants/pipeline-summary');
      return data;
    } catch (e) {
      console.error('recruitmentService.getPipelineSummary failed:', e);
      return [];
    }
  },

  async createApplicant(applicantData: Partial<CourierApplicant>): Promise<CourierApplicant> {
    const { data } = await api.post<CourierApplicant>('/applicants', applicantData);
    return data;
  },

  async advanceStage(id: number): Promise<CourierApplicant | undefined> {
    try {
      const { data } = await api.post<CourierApplicant>(`/applicants/${id}/advance`);
      return data;
    } catch (e) {
      console.error('recruitmentService.advanceStage failed:', e);
      return undefined;
    }
  },

  async rejectApplicant(id: number, reason: string): Promise<CourierApplicant | undefined> {
    try {
      const { data } = await api.post<CourierApplicant>(`/applicants/${id}/reject`, { reason });
      return data;
    } catch (e) {
      console.error('recruitmentService.rejectApplicant failed:', e);
      return undefined;
    }
  },

  async approveApplicant(id: number): Promise<CourierApplicant | undefined> {
    try {
      const { data } = await api.post<CourierApplicant>(`/applicants/${id}/approve`);
      return data;
    } catch (e) {
      console.error('recruitmentService.approveApplicant failed:', e);
      return undefined;
    }
  },
};
