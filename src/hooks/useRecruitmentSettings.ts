import { useState, useEffect, useCallback } from 'react';
import { recruitmentSettingsService } from '@/services/np_recruitmentSettingsService';
import type { RecruitmentStageConfig } from '@/types';

export function useRecruitmentSettings() {
  const [stages, setStages] = useState<RecruitmentStageConfig[]>([]);
  const [loading, setLoading] = useState(true);

  const refresh = useCallback(async () => {
    setLoading(true);
    try {
      const data = await recruitmentSettingsService.getStages();
      setStages(data);
    } catch (e) {
      console.error('useRecruitmentSettings refresh failed:', e);
    } finally {
      setLoading(false);
    }
  }, []);

  useEffect(() => { refresh(); }, [refresh]);

  const createStage = useCallback(async (data: Partial<RecruitmentStageConfig>) => {
    await recruitmentSettingsService.createStage(data);
    await refresh();
  }, [refresh]);

  const updateStage = useCallback(async (id: number, data: Partial<RecruitmentStageConfig>) => {
    await recruitmentSettingsService.updateStage(id, data);
    await refresh();
  }, [refresh]);

  const deleteStage = useCallback(async (id: number) => {
    await recruitmentSettingsService.deleteStage(id);
    await refresh();
  }, [refresh]);

  const seedDefaults = useCallback(async () => {
    await recruitmentSettingsService.seedStages();
    await refresh();
  }, [refresh]);

  return { stages, loading, createStage, updateStage, deleteStage, seedDefaults, refresh };
}
