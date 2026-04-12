import api from './np_api';

export interface ReportData {
  jobsCompleted: number;
  onTimePercent: number;
  revenue: string;
  dailyVolume: { day: string; value: number }[];
  [key: string]: unknown;
}

const DEFAULT_REPORT: ReportData = {
  jobsCompleted: 0,
  onTimePercent: 0,
  revenue: '$0',
  dailyVolume: [],
};

/**
 * @backend-needed GET /v1/np/reports — Loc: build this endpoint
 * Query params: from (ISO date), to (ISO date)
 */
export const reportService = {
  /** @backend-needed GET /v1/np/reports */
  async getData(from: string, to: string): Promise<ReportData> {
    try {
      const { data } = await api.get<ReportData>('/v1/np/reports', { params: { from, to } });
      return data ?? DEFAULT_REPORT;
    } catch (e: any) {
      if (e?.response?.status === 404 || e?.code === 'ERR_NETWORK') {
        // FALLBACK: returns empty report until backend is implemented
        return DEFAULT_REPORT;
      }
      console.error('reportService.getData failed:', e);
      return DEFAULT_REPORT;
    }
  },
};
