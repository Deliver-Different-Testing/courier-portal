import api from './np_api';
import type { Courier } from '@/types';

export async function getAllCouriersWithApproval(): Promise<Courier[]> {
  try {
    const { data } = await api.get<Courier[]>('/driver-approvals');
    return data;
  } catch (e) {
    console.error('getAllCouriersWithApproval failed:', e);
    return [];
  }
}

export async function getPendingDrivers(): Promise<Courier[]> {
  try {
    const { data } = await api.get<Courier[]>('/driver-approvals', { params: { status: 'pending_approval' } });
    return data;
  } catch (e) {
    console.error('getPendingDrivers failed:', e);
    return [];
  }
}

export async function getApprovedDrivers(): Promise<Courier[]> {
  try {
    const { data } = await api.get<Courier[]>('/driver-approvals', { params: { status: 'approved' } });
    return data;
  } catch (e) {
    console.error('getApprovedDrivers failed:', e);
    return [];
  }
}

export async function getPendingCount(): Promise<number> {
  try {
    const { data } = await api.get<{ count: number }>('/driver-approvals/pending-count');
    return data.count;
  } catch (e) {
    console.error('getPendingCount failed:', e);
    return 0;
  }
}

export async function flagForApproval(courierId: number, profileId: number): Promise<void> {
  await api.post(`/driver-approvals/${courierId}/flag`, { profileId });
}

export async function approveDriver(courierId: number, notes?: string): Promise<void> {
  await api.put(`/driver-approvals/${courierId}/approve`, { notes });
}

export async function rejectDriver(courierId: number, notes: string): Promise<void> {
  await api.put(`/driver-approvals/${courierId}/reject`, { notes });
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
