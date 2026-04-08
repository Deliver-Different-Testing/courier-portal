import api from './np_api';

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

export const openforceService = {
  async getSettings(): Promise<OfSettings> {
    try {
      const { data } = await api.get<OfSettings>('/openforce/settings');
      return data;
    } catch (e) {
      console.error('openforceService.getSettings failed:', e);
      return {
        clientId: '', clientGuid: '', accessKey: '', apiKey: '',
        activationCodeIC: '', activationCodeMaster: '', activationCodeSub: '',
        connected: false, lastSync: null,
      };
    }
  },

  async saveSettings(settings: OfSettings): Promise<void> {
    await api.put('/openforce/settings', settings);
  },

  async testConnection(settings: OfSettings): Promise<boolean> {
    try {
      const { data } = await api.post<{ connected: boolean }>('/openforce/test-connection', settings);
      return data.connected;
    } catch (e) {
      console.error('openforceService.testConnection failed:', e);
      return false;
    }
  },

  async getApiLog(): Promise<OfApiLogEntry[]> {
    try {
      const { data } = await api.get<OfApiLogEntry[]>('/openforce/activity');
      return data;
    } catch (e) {
      console.error('openforceService.getApiLog failed:', e);
      return [];
    }
  },

  async clearApiLog(): Promise<void> {
    try {
      await api.delete('/openforce/activity');
    } catch (e) {
      console.error('openforceService.clearApiLog failed:', e);
    }
  },
};
