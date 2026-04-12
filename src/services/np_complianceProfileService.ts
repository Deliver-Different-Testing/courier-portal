import type { ComplianceProfile, DriverComplianceStatus } from '@/types';
import { mockComplianceProfiles, mockDriverComplianceStatuses } from './np_mockData';

const PROFILES_KEY = 'np_compliance_profiles';
const DRIVER_STATUS_KEY = 'np_driver_compliance_statuses_v2';
const RECRUITMENT_CONFIG_KEY = 'np_tenant_recruitment_config';

function loadProfiles(): ComplianceProfile[] {
  try {
    const raw = localStorage.getItem(PROFILES_KEY);
    if (raw) return JSON.parse(raw);
  } catch { /* ignore */ }
  const seed = [...mockComplianceProfiles];
  localStorage.setItem(PROFILES_KEY, JSON.stringify(seed));
  return seed;
}

function saveProfiles(profiles: ComplianceProfile[]) {
  localStorage.setItem(PROFILES_KEY, JSON.stringify(profiles));
}

function loadDriverStatuses(): DriverComplianceStatus[] {
  try {
    const raw = localStorage.getItem(DRIVER_STATUS_KEY);
    if (raw) return JSON.parse(raw);
  } catch { /* ignore */ }
  const seed = [...mockDriverComplianceStatuses];
  localStorage.setItem(DRIVER_STATUS_KEY, JSON.stringify(seed));
  return seed;
}

export interface RecruitmentConfig {
  recruitmentViewMode: 'full_pipeline' | 'ready_for_review';
  visibleStages?: string[];
}

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
  getAll(): ComplianceProfile[] {
    return loadProfiles();
  },

  getById(id: number): ComplianceProfile | undefined {
    return loadProfiles().find(p => p.id === id);
  },

  create(profile: Omit<ComplianceProfile, 'id' | 'createdDate'>): ComplianceProfile {
    const profiles = loadProfiles();
    const newId = Math.max(0, ...profiles.map(p => p.id)) + 1;
    const newProfile: ComplianceProfile = {
      ...profile,
      id: newId,
      createdDate: new Date().toISOString(),
    };
    if (newProfile.isDefault) {
      profiles.forEach(p => p.isDefault = false);
    }
    profiles.push(newProfile);
    saveProfiles(profiles);
    return newProfile;
  },

  update(id: number, updates: Partial<ComplianceProfile>): ComplianceProfile | undefined {
    const profiles = loadProfiles();
    const idx = profiles.findIndex(p => p.id === id);
    if (idx === -1) return undefined;
    if (updates.isDefault) {
      profiles.forEach(p => p.isDefault = false);
    }
    profiles[idx] = { ...profiles[idx], ...updates, modifiedDate: new Date().toISOString() };
    saveProfiles(profiles);
    return profiles[idx];
  },

  delete(id: number): boolean {
    const profiles = loadProfiles();
    const filtered = profiles.filter(p => p.id !== id);
    if (filtered.length === profiles.length) return false;
    saveProfiles(filtered);
    return true;
  },

  getDriverStatuses(): DriverComplianceStatus[] {
    return loadDriverStatuses();
  },

  getDriverStatus(courierId: number): DriverComplianceStatus | undefined {
    return loadDriverStatuses().find(d => d.courierId === courierId);
  },

  getEligibleDriverCount(profileId: number): number {
    return loadDriverStatuses().filter(d =>
      d.profiles.some(p => p.profileId === profileId && p.isEligible)
    ).length;
  },

  getRecruitmentConfig(): RecruitmentConfig {
    return loadRecruitmentConfig();
  },

  setRecruitmentConfig(config: RecruitmentConfig) {
    saveRecruitmentConfig(config);
  },
};
