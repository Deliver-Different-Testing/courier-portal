import api from './np_api';
import type { Courier } from '@/types';

/**
 * @backend-needed GET /v1/np/driver-approvals — Loc: build this endpoint
 * @backend-needed PUT /v1/np/driver-approvals/{courierId} — Loc: build this endpoint
 */

/** @backend-needed GET /v1/np/driver-approvals */
export async function getAllCouriersWithApproval(): Promise<Courier[]> {
  try {
    const { data } = await api.get<Courier[]>('/v1/np/driver-approvals');
    return data ?? [];
  } catch (e: any) {
    if (e?.response?.status === 404 || e?.code === 'ERR_NETWORK') {
      // FALLBACK: returns empty array until backend is implemented
      return [];
    }
    console.error('getAllCouriersWithApproval failed:', e);
    return [];
  }
}

/** @backend-needed GET /v1/np/driver-approvals?status=pending_approval */
export async function getPendingDrivers(): Promise<Courier[]> {
  try {
    const { data } = await api.get<Courier[]>('/v1/np/driver-approvals', { params: { status: 'pending_approval' } });
    return data ?? [];
  } catch (e: any) {
    if (e?.response?.status === 404 || e?.code === 'ERR_NETWORK') {
      return [];
    }
    console.error('getPendingDrivers failed:', e);
    return [];
  }
}

/** @backend-needed GET /v1/np/driver-approvals?status=approved */
export async function getApprovedDrivers(): Promise<Courier[]> {
  try {
    const { data } = await api.get<Courier[]>('/v1/np/driver-approvals', { params: { status: 'approved' } });
    return data ?? [];
  } catch (e: any) {
    if (e?.response?.status === 404 || e?.code === 'ERR_NETWORK') {
      return [];
    }
    console.error('getApprovedDrivers failed:', e);
    return [];
  }
}

/** @backend-needed GET /v1/np/driver-approvals?status=pending_approval — count only */
export async function getPendingCount(): Promise<number> {
  const pending = await getPendingDrivers();
  return pending.length;
}

/** @backend-needed PUT /v1/np/driver-approvals/{courierId} — flag for approval */
export async function flagForApproval(courierId: number, profileId: number): Promise<void> {
  await api.put(`/v1/np/driver-approvals/${courierId}`, {
    tenantApprovalStatus: 'pending_approval',
    complianceProfileId: profileId,
  });
}

/** @backend-needed PUT /v1/np/driver-approvals/{courierId} — approve */
export async function approveDriver(courierId: number, notes?: string): Promise<void> {
  await api.put(`/v1/np/driver-approvals/${courierId}`, {
    tenantApprovalStatus: 'approved',
    tenantApprovalNotes: notes ?? null,
    tenantApprovalDate: new Date().toISOString(),
  });
}

/** @backend-needed PUT /v1/np/driver-approvals/{courierId} — reject */
export async function rejectDriver(courierId: number, notes: string): Promise<void> {
  await api.put(`/v1/np/driver-approvals/${courierId}`, {
    tenantApprovalStatus: 'rejected',
    tenantApprovalNotes: notes,
    tenantApprovalDate: new Date().toISOString(),
  });
}

export const driverApprovalService = {
  getAllCouriersWithApproval,
  getPendingDrivers,
  getApprovedDrivers,
  getPendingCount,
  flagForApproval,
  approveDriver,
  rejectDriver,
};
