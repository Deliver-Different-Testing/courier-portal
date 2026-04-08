import { couriers, activityFeed } from './np_mockData';

export const dashboardService = {
  getStats: () => ({
    activeCouriers: couriers.filter(c => c.status === 'active').length,
    jobsToday: 34,
    completed: 28,
    revenueThisWeek: '$12,450',
  }),

  getComplianceAlerts: () => {
    const expiring = couriers.filter(c => c.compliance === 'warning').length +
      couriers.filter(c => c.documents.some(d => d.status === 'expiring')).length;
    return expiring;
  },

  getActivityFeed: () => activityFeed,
};
