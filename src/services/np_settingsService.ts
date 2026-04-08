import api from './np_api';

export interface AppSettings {
  name: string;
  code: string;
  address: string;
  phone: string;
  email: string;
  coverageAreas: string[];
  notifications: Record<string, boolean>;
}

export const settingsService = {
  async getSettings(): Promise<AppSettings> {
    try {
      const { data } = await api.get<AppSettings>('/settings');
      return data;
    } catch (e) {
      console.error('settingsService.getSettings failed:', e);
      return {
        name: '', code: '', address: '', phone: '', email: '',
        coverageAreas: [], notifications: {},
      };
    }
  },

  async updateSettings(settings: Partial<AppSettings>): Promise<AppSettings> {
    const { data } = await api.put<AppSettings>('/settings', settings);
    return data;
  },
};
