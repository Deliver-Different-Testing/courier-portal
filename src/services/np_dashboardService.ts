import api from './np_api';

export interface DashboardStats {
  activeCouriers: number;
  jobsToday: number;
  completed: number;
  revenueThisWeek: string;
}

export interface ActivityFeedItem {
  id: string | number;
  type: string;
  message?: string;
  time?: string;
  description?: string;
  timestamp?: string;
  [key: string]: string | number | boolean | null | undefined;
}

export interface DashboardData {
  stats: DashboardStats;
  complianceAlerts: number;
  activityFeed: ActivityFeedItem[];
}

/**
 * @backend-needed GET /v1/np/dashboard — Loc: build this endpoint
 * Returns NP dashboard stats, compliance alert count, and activity feed.
 */
export const dashboardService = {
  async getDashboard(): Promise<DashboardData> {
    try {
      const { data } = await api.get<DashboardData>('/v1/np/dashboard');
      return data;
    } catch (e: any) {
      if (e?.response?.status === 404 || e?.code === 'ERR_NETWORK') {
        // FALLBACK: returns empty dashboard until backend is implemented
        return {
          stats: { activeCouriers: 0, jobsToday: 0, completed: 0, revenueThisWeek: '$0' },
          complianceAlerts: 0,
          activityFeed: [],
        };
      }
      console.error('dashboardService.getDashboard failed:', e);
      return {
        stats: { activeCouriers: 0, jobsToday: 0, completed: 0, revenueThisWeek: '$0' },
        complianceAlerts: 0,
        activityFeed: [],
      };
    }
  },

  // Legacy sync-style wrappers for pages that haven't been updated yet
  getStats: () => ({
    activeCouriers: 0,
    jobsToday: 0,
    completed: 0,
    revenueThisWeek: '$0',
  }),

  getComplianceAlerts: () => 0,

  getActivityFeed: () => [] as ActivityFeedItem[],
};
