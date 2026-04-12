import type { Courier } from '@/types';
import { couriers as mockCouriers } from './np_mockData';

const STORAGE_KEY = 'np_driver_approval_overrides';

interface ApprovalOverride {
  courierId: number;
  tenantApprovalStatus: Courier['tenantApprovalStatus'];
  tenantApprovalDate?: string;
  tenantApprovalNotes?: string;
  complianceProfileId?: number;
}

function loadOverrides(): ApprovalOverride[] {
  try {
    const raw = localStorage.getItem(STORAGE_KEY);
    if (raw) return JSON.parse(raw);
  } catch { /* ignore */ }
  return [];
}

function saveOverrides(overrides: ApprovalOverride[]) {
  localStorage.setItem(STORAGE_KEY, JSON.stringify(overrides));
}

function applyCourierOverrides(courier: Courier): Courier {
  const overrides = loadOverrides();
  const override = overrides.find(o => o.courierId === courier.id);
  if (!override) return courier;
  return {
    ...courier,
    tenantApprovalStatus: override.tenantApprovalStatus,
    tenantApprovalDate: override.tenantApprovalDate,
    tenantApprovalNotes: override.tenantApprovalNotes,
    complianceProfileId: override.complianceProfileId ?? courier.complianceProfileId,
  };
}

export function getAllCouriersWithApproval(): Courier[] {
  return mockCouriers.map(applyCourierOverrides);
}

export function getPendingDrivers(): Courier[] {
  return getAllCouriersWithApproval().filter(c => c.tenantApprovalStatus === 'pending_approval');
}

export function getApprovedDrivers(): Courier[] {
  return getAllCouriersWithApproval().filter(c => c.tenantApprovalStatus === 'approved');
}

export function getPendingCount(): number {
  return getPendingDrivers().length;
}

export function flagForApproval(courierId: number, profileId: number): void {
  const overrides = loadOverrides();
  const idx = overrides.findIndex(o => o.courierId === courierId);
  const entry: ApprovalOverride = {
    courierId,
    tenantApprovalStatus: 'pending_approval',
    complianceProfileId: profileId,
  };
  if (idx >= 0) overrides[idx] = entry;
  else overrides.push(entry);
  saveOverrides(overrides);
}

export function approveDriver(courierId: number, notes?: string): void {
  const overrides = loadOverrides();
  const idx = overrides.findIndex(o => o.courierId === courierId);
  const existing = idx >= 0 ? overrides[idx] : { courierId, tenantApprovalStatus: 'approved' as const };
  const entry: ApprovalOverride = {
    ...existing,
    tenantApprovalStatus: 'approved',
    tenantApprovalDate: new Date().toISOString(),
    tenantApprovalNotes: notes || undefined,
  };
  if (idx >= 0) overrides[idx] = entry;
  else overrides.push(entry);
  saveOverrides(overrides);
}

export function rejectDriver(courierId: number, notes: string): void {
  const overrides = loadOverrides();
  const idx = overrides.findIndex(o => o.courierId === courierId);
  const existing = idx >= 0 ? overrides[idx] : { courierId, tenantApprovalStatus: 'rejected' as const };
  const entry: ApprovalOverride = {
    ...existing,
    tenantApprovalStatus: 'rejected',
    tenantApprovalDate: new Date().toISOString(),
    tenantApprovalNotes: notes,
  };
  if (idx >= 0) overrides[idx] = entry;
  else overrides.push(entry);
  saveOverrides(overrides);
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
