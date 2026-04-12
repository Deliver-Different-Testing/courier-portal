/** @backend Loc: register this service in Program.cs */
import api from './np_api';

// ─── Types (inlined here — no longer imported from np_schedulingMockData) ──

export interface VehicleSummary {
  vehicle: string;
  available: number;
  total: number;
}

export interface ScheduleSummary {
  id: number;
  created: string;
  bookDate: string;
  location: string;
  name: string;
  notificationSent: string | null;
  startTime: string; // "HH:mm:ss"
  endTime: string;
  wanted: number;
  available: number;
  vehicleSummaries: VehicleSummary[];
}

export interface TimeSlotVehicle {
  id: number;
  location: string;
  bookDateTime: string; // ISO datetime
  wanted: number | null;
  vehicleTypes: string[];
}

export interface LocationSummary {
  location: string;
  totalCouriers: number;
  totalAvailable: number;
  scheduleSummaries: ScheduleSummary[];
  timeSlots: TimeSlotVehicle[];
}

export interface CourierDetails {
  id: number;
  code: string;
  firstName: string;
  surname: string;
  mobile: string;
  vehicleType: string;
  region: string;
  active: boolean;
}

export interface ScheduleResponse {
  id: number;
  created: string;
  updated: string;
  statusId: number; // 0=pending(no response), 1=available, 2=unavailable, 3=cancelled
  status: string;
  timeSlot: { id: number; location: string; bookDateTime: string; wanted: number | null } | null;
}

export interface CourierBySchedule {
  courier: CourierDetails;
  scheduleResponse: ScheduleResponse | null;
}

export interface ScheduleDto {
  id: number;
  created: string;
  bookDate: string;
  location: string;
  name: string;
  notificationSent: string | null;
  startTime: string;
  endTime: string;
  wanted: number;
}

/**
 * Courier scheduling — wired to SchedulingController.
 * Backend route: api/v1/np/scheduling
 */
export const schedulingService = {
  /** @backend GET /api/v1/np/scheduling/locations */
  async getLocationSummaries(): Promise<LocationSummary[]> {
    try {
      const { data } = await api.get<LocationSummary[]>('/v1/np/scheduling/locations');
      return data ?? [];
    } catch (e) {
      console.error('schedulingService.getLocationSummaries failed:', e);
      return [];
    }
  },

  /** @backend GET /api/v1/np/scheduling/schedules/{id}/responses */
  async getCourierResponses(scheduleId: number): Promise<CourierBySchedule[]> {
    try {
      const { data } = await api.get<CourierBySchedule[]>(`/v1/np/scheduling/schedules/${scheduleId}/responses`);
      return data ?? [];
    } catch (e) {
      console.error(`schedulingService.getCourierResponses(${scheduleId}) failed:`, e);
      return [];
    }
  },

  /** @backend GET /api/v1/np/scheduling/pending-notifications */
  async getPendingNotifications(): Promise<ScheduleDto[]> {
    try {
      const { data } = await api.get<ScheduleDto[]>('/v1/np/scheduling/pending-notifications');
      return data ?? [];
    } catch (e) {
      console.error('schedulingService.getPendingNotifications failed:', e);
      return [];
    }
  },

  /** @backend POST /api/v1/np/scheduling/schedules */
  async createSchedule(schedule: Partial<ScheduleDto>): Promise<ScheduleDto> {
    const { data } = await api.post<ScheduleDto>('/v1/np/scheduling/schedules', schedule);
    return data;
  },

  /** @backend POST /api/v1/np/scheduling/schedules/{id}/notify */
  async sendNotification(scheduleId: number): Promise<void> {
    await api.post(`/v1/np/scheduling/schedules/${scheduleId}/notify`, {});
  },

  /** @backend POST /api/v1/np/scheduling/schedules/notify-batch */
  async sendNotificationBatch(scheduleIds: number[]): Promise<void> {
    await api.post('/v1/np/scheduling/schedules/notify-batch', { scheduleIds });
  },
};
