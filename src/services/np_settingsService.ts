import api from './np_api';

export interface NpSettings {
  name: string;
  code: string;
  address: string;
  phone: string;
  email: string;
  coverageAreas: string[];
  notifications: Record<string, boolean>;
  [key: string]: unknown;
}

const DEFAULT_SETTINGS: NpSettings = {
  name: '',
  code: '',
  address: '',
  phone: '',
  email: '',
  coverageAreas: [],
  notifications: {},
};

/**
 * @backend-needed GET /v1/np/settings — Loc: build this endpoint
 * @backend-needed PUT /v1/np/settings — Loc: build this endpoint
 */
export const settingsService = {
  /** @backend-needed GET /v1/np/settings */
  async getSettings(): Promise<NpSettings> {
    try {
      const { data } = await api.get<NpSettings>('/v1/np/settings');
      return data ?? DEFAULT_SETTINGS;
    } catch (e: any) {
      if (e?.response?.status === 404 || e?.code === 'ERR_NETWORK') {
        // FALLBACK: returns empty settings until backend is implemented
        return DEFAULT_SETTINGS;
      }
      console.error('settingsService.getSettings failed:', e);
      return DEFAULT_SETTINGS;
    }
  },

  /** @backend-needed PUT /v1/np/settings */
  async saveSettings(settings: Partial<NpSettings>): Promise<NpSettings> {
    const { data } = await api.put<NpSettings>('/v1/np/settings', settings);
    return data;
  },
};
