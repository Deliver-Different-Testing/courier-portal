import api from './np_api';
import type { ComplianceProfile, DriverComplianceStatus } from '@/types';

export interface RecruitmentConfig {
  recruitmentViewMode: 'full_pipeline' | 'ready_for_review';
  visibleStages?: string[];
}

export const complianceProfileService = {
  async getAll(): Promise<ComplianceProfile[]> {
    try {
      const { data } = await api.get<ComplianceProfile[]>('/compliance-profiles');
      return data;
    } catch (e) {
      console.error('complianceProfileService.getAll failed:', e);
      return [];
    }
  },

  async getById(id: number): Promise<ComplianceProfile | undefined> {
    try {
      const { data } = await api.get<ComplianceProfile>(`/compliance-profiles/${id}`);
      return data;
    } catch (e) {
      console.error('complianceProfileService.getById failed:', e);
      return undefined;
    }
  },

  async create(profile: Omit<ComplianceProfile, 'id' | 'createdDate'>): Promise<ComplianceProfile> {
    const { data } = await api.post<ComplianceProfile>('/compliance-profiles', profile);
    return data;
  },

  async update(id: number, updates: Partial<ComplianceProfile>): Promise<ComplianceProfile | undefined> {
    try {
      const { data } = await api.put<ComplianceProfile>(`/compliance-profiles/${id}`, updates);
      return data;
    } catch (e) {
      console.error('complianceProfileService.update failed:', e);
      return undefined;
    }
  },

  async delete(id: number): Promise<boolean> {
    try {
      await api.delete(`/compliance-profiles/${id}`);
      return true;
    } catch (e) {
      console.error('complianceProfileService.delete failed:', e);
      return false;
    }
  },

  async getDriverStatuses(): Promise<DriverComplianceStatus[]> {
    try {
      const { data } = await api.get<DriverComplianceStatus[]>('/compliance-profiles/driver-statuses');
      return data;
    } catch (e) {
      console.error('complianceProfileService.getDriverStatuses failed:', e);
      return [];
    }
  },

  async getDriverStatus(courierId: number): Promise<DriverComplianceStatus | undefined> {
    try {
      const { data } = await api.get<DriverComplianceStatus>(`/compliance-profiles/driver-statuses/${courierId}`);
      return data;
    } catch (e) {
      console.error('complianceProfileService.getDriverStatus failed:', e);
      return undefined;
    }
  },

  async getEligibleDriverCount(profileId: number): Promise<number> {
    try {
      const { data } = await api.get<{ count: number }>(`/compliance-profiles/${profileId}/eligible-count`);
      return data.count;
    } catch (e) {
      console.error('complianceProfileService.getEligibleDriverCount failed:', e);
      return 0;
    }
  },

  async getRecruitmentConfig(): Promise<RecruitmentConfig> {
    try {
      const { data } = await api.get<RecruitmentConfig>('/compliance-profiles/recruitment-config');
      return data;
    } catch (e) {
      console.error('complianceProfileService.getRecruitmentConfig failed:', e);
      return { recruitmentViewMode: 'full_pipeline' };
    }
  },

  async setRecruitmentConfig(config: RecruitmentConfig): Promise<void> {
    try {
      await api.put('/compliance-profiles/recruitment-config', config);
    } catch (e) {
      console.error('complianceProfileService.setRecruitmentConfig failed:', e);
    }
  },
};
