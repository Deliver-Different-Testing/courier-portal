/** @backend Loc: register this service in Program.cs */
import portalApi from './portal_api';

// ─── Types (inlined here so consumers don't import from portal_mockData) ──

export type Country = 'NZ' | 'US';

export interface CourierProfile {
  id: number;
  code: string;
  firstName: string;
  surname: string;
  email: string;
  mobile: string;
  taxNo: string;
  bankAccountNo: string;
  bankRoutingNumber?: string;
  address: string;
  isMasterCourier: boolean;
  country: Country;
}

export interface TimeSlot {
  id: number;
  bookDateTime: string; // ISO
  remaining: number | null;
  wanted: boolean;
}

export interface Schedule {
  id: number;
  name: string;
  location: string;
  bookDate: string; // ISO date
  startTime: string; // HH:mm:ss
  endTime: string; // HH:mm:ss
  wanted: number;
  timeSlots: TimeSlot[];
  hasTimeSlots: boolean;
  response: { statusId: 1 | 2 | 3; timeSlot?: TimeSlot } | null;
}

export interface Job {
  id: number;
  pickupAddress: string;
  deliveryAddress: string;
  status: 'Pending' | 'In Transit' | 'Delivered' | 'Failed';
  podSigned: boolean;
  podSignedAt?: string;
  notes?: string;
}

export interface Run {
  id: number;
  name: string;
  location: string;
  bookDate: string;
  startTime: string;
  endTime: string;
  status: 'Upcoming' | 'In Progress' | 'Completed';
  jobs: Job[];
}

export interface ReportSummary {
  totalRuns: number;
  totalEarnings: number;
  avgPerRun: number;
  thisWeekRuns: number;
  thisWeekEarnings: number;
  thisMonthRuns: number;
  thisMonthEarnings: number;
  weeklyData: { week: string; runs: number; earnings: number }[];
}

export interface Subcontractor {
  id: number;
  name: string;
  code: string;
  phone: string;
  runsCompleted: number;
  totalEarnings: number;
  status: 'Active' | 'Inactive';
}

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
