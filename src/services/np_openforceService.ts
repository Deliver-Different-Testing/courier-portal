import api from './np_api';

// ─── Types ─────────────────────────────────────────────────────────────────

export interface OfSettings {
  clientId: string;
  clientGuid: string;
  accessKey: string;
  apiKey: string;
  activationCodeIC: string;
  activationCodeMaster: string;
  activationCodeSub: string;
  connected: boolean;
  lastSync: string | null;
}

export interface OfApiLogEntry {
  id: string;
  timestamp: string;
  direction: 'Outbound' | 'Inbound';
  endpoint: string;
  status: number;
  success: boolean;
  durationMs: number;
  responseSummary: string;
  category: 'invitation' | 'settlement' | 'webhook' | 'contractor' | 'auth' | 'other';
}

export interface OpenforceRecruitmentStatus {
  pending: number;
  active: number;
  rejected: number;
  total: number;
}

const defaultSettings: OfSettings = {
  clientId: '',
  clientGuid: '',
  accessKey: '',
  apiKey: '',
  activationCodeIC: '',
  activationCodeMaster: '',
  activationCodeSub: '',
  connected: false,
  lastSync: null,
};

export const openforceService = {
  /**
   * @backend-needed GET /v1/np/openforce/recruitment-status — Loc: build this endpoint
   * Returns Openforce recruitment status counts.
   */
  async getRecruitmentStatus(): Promise<OpenforceRecruitmentStatus> {
    try {
      const { data } = await api.get<OpenforceRecruitmentStatus>('/v1/np/openforce/recruitment-status');
      return data;
    } catch (e: any) {
      if (e?.response?.status === 404 || e?.code === 'ERR_NETWORK') {
        // FALLBACK: returns empty counts until backend is implemented
        return { pending: 0, active: 0, rejected: 0, total: 0 };
      }
      console.error('openforceService.getRecruitmentStatus failed:', e);
      return { pending: 0, active: 0, rejected: 0, total: 0 };
    }
  },

  getSettings(): OfSettings {
    const raw = localStorage.getItem('np_openforce_settings');
    if (raw) return JSON.parse(raw);
    return { ...defaultSettings };
  },

  saveSettings(settings: OfSettings): void {
    localStorage.setItem('np_openforce_settings', JSON.stringify(settings));
  },

  async testConnection(settings: OfSettings): Promise<boolean> {
    // Simulate API test — connected if clientId + apiKey provided
    await new Promise(r => setTimeout(r, 1200));
    return !!(settings.clientId && settings.apiKey && settings.accessKey);
  },

  getApiLog(): OfApiLogEntry[] {
    const raw = localStorage.getItem('np_openforce_api_log');
    if (raw) return JSON.parse(raw);
    // Return empty — no auto-seed of mock data
    return [];
  },

  clearApiLog(): void {
    localStorage.removeItem('np_openforce_api_log');
  },
};
