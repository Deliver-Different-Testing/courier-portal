import api from './np_api';

export interface DashboardStats {
  activeCouriers: number;
  jobsToday: number;
  completed: number;
  revenueThisWeek: string;
}

export const dashboardService = {
  async getStats(): Promise<DashboardStats> {
    try {
      const { data } = await api.get<DashboardStats>('/dashboard/stats');
      return data;
    } catch (e) {
      console.error('dashboardService.getStats failed:', e);
      return { activeCouriers: 0, jobsToday: 0, completed: 0, revenueThisWeek: '$0' };
    }
  },

  async getComplianceAlerts(): Promise<number> {
    try {
      const { data } = await api.get<{ count: number }>('/dashboard/compliance-alerts');
      return data.count;
    } catch (e) {
      console.error('dashboardService.getComplianceAlerts failed:', e);
      return 0;
    }
  },

  async getActivityFeed(): Promise<any[]> {
    try {
      const { data } = await api.get<any[]>('/dashboard/activity');
      return data;
    } catch (e) {
      console.error('dashboardService.getActivityFeed failed:', e);
      return [];
    }
  },
};
