// @ts-nocheck
import { dashboardService } from '@/services/np_dashboardService';

export function useDashboard() {
  const stats = dashboardService.getStats();
  const complianceAlerts = dashboardService.getComplianceAlerts();
  const activity = dashboardService.getActivityFeed();
  return { stats, complianceAlerts, activity };
}
