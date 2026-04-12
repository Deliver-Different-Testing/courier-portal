/** @backend Loc: register this service in Program.cs */
import api from './np_api';
import type { ComplianceProfile, DriverComplianceStatus } from '@/types';

export interface RecruitmentConfig {
  recruitmentViewMode: 'full_pipeline' | 'ready_for_review';
  visibleStages?: string[];
}

const RECRUITMENT_CONFIG_KEY = 'np_tenant_recruitment_config';

function loadRecruitmentConfig(): RecruitmentConfig {
  try {
    const raw = localStorage.getItem(RECRUITMENT_CONFIG_KEY);
    if (raw) return JSON.parse(raw);
  } catch { /* ignore */ }
  return { recruitmentViewMode: 'full_pipeline' };
}

function saveRecruitmentConfig(config: RecruitmentConfig) {
  localStorage.setItem(RECRUITMENT_CONFIG_KEY, JSON.stringify(config));
}

export const complianceProfileService = {
  /** @backend GET /api/v1/settings/compliance-profiles — Loc: build this endpoint */
  async getAll(): Promise<ComplianceProfile[]> {
    try {
      const { data } = await api.get<ComplianceProfile[]>('/v1/settings/compliance-profiles');
      return data ?? [];
    } catch (e) {
      console.error('complianceProfileService.getAll failed:', e);
      return [];
    }
  },

  /** @backend GET /api/v1/settings/compliance-profiles/{id} — Loc: build this endpoint */
  async getById(id: number): Promise<ComplianceProfile | undefined> {
    try {
      const { data } = await api.get<ComplianceProfile>(`/v1/settings/compliance-profiles/${id}`);
      return data;
    } catch (e) {
      console.error(`complianceProfileService.getById(${id}) failed:`, e);
      return undefined;
    }
  },

  /** @backend POST /api/v1/settings/compliance-profiles — Loc: build this endpoint */
  async create(profile: Omit<ComplianceProfile, 'id' | 'createdDate'>): Promise<ComplianceProfile | undefined> {
    try {
      const { data } = await api.post<ComplianceProfile>('/v1/settings/compliance-profiles', profile);
      return data;
    } catch (e) {
      console.error('complianceProfileService.create failed:', e);
      return undefined;
    }
  },

  /** @backend PUT /api/v1/settings/compliance-profiles/{id} — Loc: build this endpoint */
  async update(id: number, updates: Partial<ComplianceProfile>): Promise<ComplianceProfile | undefined> {
    try {
      const { data } = await api.put<ComplianceProfile>(`/v1/settings/compliance-profiles/${id}`, updates);
      return data;
    } catch (e) {
      console.error(`complianceProfileService.update(${id}) failed:`, e);
      return undefined;
    }
  },

  /** @backend DELETE /api/v1/settings/compliance-profiles/{id} — Loc: build this endpoint */
  async delete(id: number): Promise<boolean> {
    try {
      await api.delete(`/v1/settings/compliance-profiles/${id}`);
      return true;
    } catch (e) {
      console.error(`complianceProfileService.delete(${id}) failed:`, e);
      return false;
    }
  },

  /** @backend GET /api/v1/np/compliance/driver-statuses — Loc: build this endpoint */
  async getDriverStatuses(): Promise<DriverComplianceStatus[]> {
    try {
      const { data } = await api.get<DriverComplianceStatus[]>('/v1/np/compliance/driver-statuses');
      return data ?? [];
    } catch (e) {
      console.error('complianceProfileService.getDriverStatuses failed:', e);
      return [];
    }
  },

  /** @backend GET /api/v1/np/compliance/score/{courierId} */
  async getDriverStatus(courierId: number): Promise<DriverComplianceStatus | undefined> {
    try {
      const { data } = await api.get<DriverComplianceStatus>(`/v1/np/compliance/driver-status/${courierId}`);
      return data;
    } catch (e) {
      console.error(`complianceProfileService.getDriverStatus(${courierId}) failed:`, e);
      return undefined;
    }
  },

  /** @backend GET /api/v1/settings/compliance-profiles/{profileId}/eligible-count — Loc: build this endpoint */
  async getEligibleDriverCount(profileId: number): Promise<number> {
    try {
      const { data } = await api.get<{ count: number }>(`/v1/settings/compliance-profiles/${profileId}/eligible-count`);
      return data?.count ?? 0;
    } catch (e) {
      console.error('complianceProfileService.getEligibleDriverCount failed:', e);
      return 0;
    }
  },

  getRecruitmentConfig(): RecruitmentConfig {
    return loadRecruitmentConfig();
  },

  setRecruitmentConfig(config: RecruitmentConfig) {
    saveRecruitmentConfig(config);
  },
};
