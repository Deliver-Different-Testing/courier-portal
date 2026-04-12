/** @backend Loc: register this service in Program.cs */
import api from './np_api';
import type { RecruitmentStageConfig } from '@/types';

/**
 * Recruitment stage settings — wired to RecruitmentSettingsController.
 * Backend route: api/v1/settings/recruitment-stages
 */
export const recruitmentSettingsService = {
  /** @backend GET /api/v1/settings/recruitment-stages */
  async getStages(): Promise<RecruitmentStageConfig[]> {
    try {
      const { data } = await api.get<RecruitmentStageConfig[]>('/v1/settings/recruitment-stages');
      return (data ?? []).sort((a, b) => a.sortOrder - b.sortOrder);
    } catch (e) {
      console.error('recruitmentSettingsService.getStages failed:', e);
      return [];
    }
  },

  /** @backend POST /api/v1/settings/recruitment-stages */
  async createStage(dto: Partial<RecruitmentStageConfig>): Promise<RecruitmentStageConfig | undefined> {
    try {
      const { data } = await api.post<RecruitmentStageConfig>('/v1/settings/recruitment-stages', dto);
      return data;
    } catch (e) {
      console.error('recruitmentSettingsService.createStage failed:', e);
      return undefined;
    }
  },

  /** @backend PUT /api/v1/settings/recruitment-stages/{id} */
  async updateStage(id: number, dto: Partial<RecruitmentStageConfig>): Promise<RecruitmentStageConfig | undefined> {
    try {
      const { data } = await api.put<RecruitmentStageConfig>(`/v1/settings/recruitment-stages/${id}`, dto);
      return data;
    } catch (e) {
      console.error('recruitmentSettingsService.updateStage failed:', e);
      return undefined;
    }
  },

  /** @backend DELETE /api/v1/settings/recruitment-stages/{id} */
  async deleteStage(id: number): Promise<boolean> {
    try {
      await api.delete(`/v1/settings/recruitment-stages/${id}`);
      return true;
    } catch (e) {
      console.error('recruitmentSettingsService.deleteStage failed:', e);
      return false;
    }
  },

  /** @backend POST /api/v1/settings/recruitment-stages/seed */
  async seedStages(): Promise<RecruitmentStageConfig[]> {
    try {
      const { data } = await api.post<RecruitmentStageConfig[]>('/v1/settings/recruitment-stages/seed', {});
      return data ?? [];
    } catch (e) {
      console.error('recruitmentSettingsService.seedStages failed:', e);
      return [];
    }
  },
};
