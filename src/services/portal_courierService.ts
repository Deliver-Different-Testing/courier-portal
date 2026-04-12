import portalApi from './portal_api';
import type { CourierProfile, Schedule, Run, ReportSummary, Subcontractor } from './portal_mockData';

// Re-export types so consumers can import from here
export type { CourierProfile, Schedule, Run, ReportSummary, Subcontractor };

export const portalCourierService = {
  /** @backend GET /api/portal/Couriers */
  async getProfile(): Promise<CourierProfile | null> {
    try {
      const { data } = await portalApi.get<CourierProfile>('/portal/Couriers');
      return data;
    } catch (e) {
      console.error('portalCourierService.getProfile failed:', e);
      return null;
    }
  },

  /** @backend GET /api/portal/Runs */
  async getRuns(): Promise<Run[]> {
    try {
      const { data } = await portalApi.get<Run[]>('/portal/Runs');
      return data;
    } catch (e) {
      console.error('portalCourierService.getRuns failed:', e);
      return [];
    }
  },

  /** @backend GET /api/portal/Schedules */
  async getSchedules(): Promise<Schedule[]> {
    try {
      const { data } = await portalApi.get<Schedule[]>('/portal/Schedules');
      return data;
    } catch (e) {
      console.error('portalCourierService.getSchedules failed:', e);
      return [];
    }
  },

  /** @backend POST /api/portal/Schedules/Available */
  async markAvailable(scheduleId: number, timeSlotId?: number): Promise<void> {
    await portalApi.post('/portal/Schedules/Available', { scheduleId, timeSlotId });
  },

  /** @backend POST /api/portal/Schedules/Unavailable */
  async markUnavailable(scheduleId: number): Promise<void> {
    await portalApi.post('/portal/Schedules/Unavailable', { scheduleId });
  },

  /** @backend GET /api/portal/Reports/Settings */
  async getReportSummary(): Promise<ReportSummary> {
    try {
      const { data } = await portalApi.get<ReportSummary>('/portal/Reports/Settings');
      return data;
    } catch (e) {
      console.error('portalCourierService.getReportSummary failed:', e);
      return { totalRuns: 0, totalEarnings: 0, avgPerRun: 0, thisWeekRuns: 0, thisWeekEarnings: 0, thisMonthRuns: 0, thisMonthEarnings: 0, weeklyData: [] };
    }
  },

  /** @backend GET /api/portal/Couriers/Contractors */
  async getSubcontractors(): Promise<Subcontractor[]> {
    try {
      const { data } = await portalApi.get<Subcontractor[]>('/portal/Couriers/Contractors');
      return data;
    } catch (e) {
      console.error('portalCourierService.getSubcontractors failed:', e);
      return [];
    }
  },
};
