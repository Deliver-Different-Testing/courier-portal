import api from './np_api';

export interface ReportData {
  jobsCompleted: number;
  onTimePercent: number;
  revenue: string;
  dailyVolume: { day: string; value: number }[];
}

export const reportService = {
  async getData(from: string, to: string): Promise<ReportData> {
    try {
      const { data } = await api.get<ReportData>('/reports/no-shows', { params: { from, to } });
      return data;
    } catch (e) {
      console.error('reportService.getData failed:', e);
      return { jobsCompleted: 0, onTimePercent: 0, revenue: '$0', dailyVolume: [] };
    }
  },

  async getSustainability(from: string, to: string): Promise<any> {
    try {
      const { data } = await api.get('/reports/sustainability', { params: { from, to } });
      return data;
    } catch (e) {
      console.error('reportService.getSustainability failed:', e);
      return null;
    }
  },
};
