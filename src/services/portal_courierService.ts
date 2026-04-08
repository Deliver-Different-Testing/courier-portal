import portalApi from './portal_api';
import type {
  CourierProfile,
  Schedule,
  Run,
  ReportSummary,
  Subcontractor,
} from './portal_devData';

export const portalCourierService = {
  async getDashboard(): Promise<{ profile: CourierProfile; reportSummary: ReportSummary; subcontractors: Subcontractor[] }> {
    try {
      const { data } = await portalApi.get('/portal/dashboard');
      return data;
    } catch (e) {
      console.error('portalCourierService.getDashboard failed:', e);
      return {
        profile: {} as CourierProfile,
        reportSummary: { totalRuns: 0, totalEarnings: 0, avgPerRun: 0, thisWeekRuns: 0, thisWeekEarnings: 0, thisMonthRuns: 0, thisMonthEarnings: 0, weeklyData: [] },
        subcontractors: [],
      };
    }
  },

  async getProfile(): Promise<CourierProfile | null> {
    try {
      const { data } = await portalApi.get<CourierProfile>('/portal/profile');
      return data;
    } catch (e) {
      console.error('portalCourierService.getProfile failed:', e);
      return null;
    }
  },

  async getRuns(): Promise<Run[]> {
    try {
      const { data } = await portalApi.get<Run[]>('/portal/runs');
      return data;
    } catch (e) {
      console.error('portalCourierService.getRuns failed:', e);
      return [];
    }
  },

  async getSchedules(): Promise<Schedule[]> {
    try {
      const { data } = await portalApi.get<Schedule[]>('/portal/schedule');
      return data;
    } catch (e) {
      console.error('portalCourierService.getSchedules failed:', e);
      return [];
    }
  },

  async respondToSchedule(scheduleId: number, statusId: 1 | 2 | 3, timeSlotId?: number): Promise<void> {
    await portalApi.post(`/portal/schedule/${scheduleId}/respond`, { statusId, timeSlotId });
  },

  async getReportSummary(): Promise<ReportSummary> {
    try {
      const { data } = await portalApi.get<ReportSummary>('/portal/reports');
      return data;
    } catch (e) {
      console.error('portalCourierService.getReportSummary failed:', e);
      return { totalRuns: 0, totalEarnings: 0, avgPerRun: 0, thisWeekRuns: 0, thisWeekEarnings: 0, thisMonthRuns: 0, thisMonthEarnings: 0, weeklyData: [] };
    }
  },

  async getSubcontractors(): Promise<Subcontractor[]> {
    try {
      const { data } = await portalApi.get<Subcontractor[]>('/portal/subcontractors');
      return data;
    } catch (e) {
      console.error('portalCourierService.getSubcontractors failed:', e);
      return [];
    }
  },
};
