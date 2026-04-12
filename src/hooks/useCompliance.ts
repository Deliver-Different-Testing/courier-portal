import { useState, useEffect, useCallback } from 'react';
import { complianceService } from '@/services/np_complianceService';
import type {
  ComplianceDashboard,
  ComplianceAlert,
  CourierComplianceScore,
  ComplianceAlertFilter,
} from '@/types';

export function useComplianceDashboard() {
  const [data, setData] = useState<ComplianceDashboard | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  const refresh = useCallback(async () => {
    setLoading(true);
    setError(null);
    try {
      setData(await complianceService.getDashboard());
    } catch (err: unknown) {
      const msg = err instanceof Error ? err.message : 'Failed to load compliance dashboard';
      setError(msg);
    } finally {
      setLoading(false);
    }
  }, []);

  useEffect(() => { refresh(); }, [refresh]);
  return { data, loading, error, refresh };
}

export function useComplianceAlerts(filters?: ComplianceAlertFilter) {
  const [alerts, setAlerts] = useState<ComplianceAlert[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  const refresh = useCallback(async () => {
    setLoading(true);
    setError(null);
    try {
      setAlerts(await complianceService.getAlerts(filters));
    } catch (err: unknown) {
      const msg = err instanceof Error ? err.message : 'Failed to load compliance alerts';
      setError(msg);
      setAlerts([]);
    } finally {
      setLoading(false);
    }
  }, [filters?.docType, filters?.status, filters?.courierName, filters?.daysAhead]);

  useEffect(() => { refresh(); }, [refresh]);
  return { alerts, loading, error, refresh };
}

export function useCourierComplianceScore(courierId: number | null) {
  const [score, setScore] = useState<CourierComplianceScore | null>(null);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const refresh = useCallback(async () => {
    if (!courierId) return;
    setLoading(true);
    setError(null);
    try {
      setScore(await complianceService.getCourierScore(courierId));
    } catch (err: unknown) {
      const msg = err instanceof Error ? err.message : 'Failed to load compliance score';
      setError(msg);
    } finally {
      setLoading(false);
    }
  }, [courierId]);

  useEffect(() => { refresh(); }, [refresh]);
  return { score, loading, error, refresh };
}
