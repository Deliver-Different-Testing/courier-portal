import type { RecruitmentStageConfig } from '@/types';
import { mockRecruitmentStages } from './np_mockData';

export const recruitmentSettingsService = {
  getStages: (): RecruitmentStageConfig[] => [...mockRecruitmentStages].sort((a, b) => a.sortOrder - b.sortOrder),

  createStage: (data: Partial<RecruitmentStageConfig>): RecruitmentStageConfig => {
    const stage: RecruitmentStageConfig = {
      id: mockRecruitmentStages.length + 1,
      tenantId: 1,
      stageName: data.stageName || '',
      sortOrder: data.sortOrder || mockRecruitmentStages.length + 1,
      enabled: data.enabled ?? true,
      mandatory: data.mandatory ?? true,
      description: data.description || null,
      createdDate: new Date().toISOString(),
    };
    mockRecruitmentStages.push(stage);
    return stage;
  },

  updateStage: (id: number, data: Partial<RecruitmentStageConfig>): RecruitmentStageConfig | undefined => {
    const s = mockRecruitmentStages.find(x => x.id === id);
    if (!s) return undefined;
    Object.assign(s, data);
    return s;
  },

  deleteStage: (id: number): boolean => {
    const idx = mockRecruitmentStages.findIndex(x => x.id === id);
    if (idx < 0) return false;
    mockRecruitmentStages.splice(idx, 1);
    return true;
  },

  seedDefaults: (): RecruitmentStageConfig[] => {
    const defaults = [
      { name: 'Registration', desc: 'Initial applicant registration' },
      { name: 'Email Verification', desc: 'Verify applicant email address' },
      { name: 'Profile', desc: 'Complete personal and vehicle profile' },
      { name: 'Documentation', desc: 'Upload required documents' },
      { name: 'Declaration/Contract', desc: 'Sign declaration and contract' },
      { name: 'Training', desc: 'Complete required training modules' },
      { name: 'Approval', desc: 'Final review and approval' },
    ];
    mockRecruitmentStages.length = 0;
    defaults.forEach((d, i) => {
      mockRecruitmentStages.push({
        id: i + 1,
        tenantId: 1,
        stageName: d.name,
        sortOrder: i + 1,
        enabled: true,
        mandatory: true,
        description: d.desc,
        createdDate: new Date().toISOString(),
      });
    });
    return [...mockRecruitmentStages];
  },
};
