import api from './np_api';
import type { RecruitmentStageConfig } from '@/types';

export const recruitmentSettingsService = {
  async getStages(): Promise<RecruitmentStageConfig[]> {
    try {
      const { data } = await api.get<RecruitmentStageConfig[]>('/recruitment-settings');
      return data;
    } catch (e) {
      console.error('recruitmentSettingsService.getStages failed:', e);
      return [];
    }
  },

  async createStage(stageData: Partial<RecruitmentStageConfig>): Promise<RecruitmentStageConfig> {
    const { data } = await api.post<RecruitmentStageConfig>('/recruitment-settings', stageData);
    return data;
  },

  async updateStage(id: number, stageData: Partial<RecruitmentStageConfig>): Promise<RecruitmentStageConfig | undefined> {
    try {
      const { data } = await api.put<RecruitmentStageConfig>(`/recruitment-settings/${id}`, stageData);
      return data;
    } catch (e) {
      console.error('recruitmentSettingsService.updateStage failed:', e);
      return undefined;
    }
  },

  async deleteStage(id: number): Promise<boolean> {
    try {
      await api.delete(`/recruitment-settings/${id}`);
      return true;
    } catch (e) {
      console.error('recruitmentSettingsService.deleteStage failed:', e);
      return false;
    }
  },

  async seedDefaults(): Promise<RecruitmentStageConfig[]> {
    try {
      const { data } = await api.post<RecruitmentStageConfig[]>('/recruitment-settings/seed');
      return data;
    } catch (e) {
      console.error('recruitmentSettingsService.seedDefaults failed:', e);
      return [];
    }
  },
};
