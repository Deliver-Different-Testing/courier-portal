import api from './np_api';
import type { Courier } from '@/types';

/**
 * @backend-needed GET /v1/np/couriers — Loc: build this endpoint
 * @backend-needed POST /v1/np/couriers — Loc: build this endpoint
 * @backend-needed PUT /v1/np/couriers/{id} — Loc: build this endpoint
 * @backend-needed DELETE /v1/np/couriers/{id} — Loc: build this endpoint
 */
export const courierService = {
  /** @backend-needed GET /v1/np/couriers */
  async getAll(): Promise<Courier[]> {
    try {
      const { data } = await api.get<Courier[]>('/v1/np/couriers');
      return data ?? [];
    } catch (e: any) {
      if (e?.response?.status === 404 || e?.code === 'ERR_NETWORK') {
        // FALLBACK: returns empty array until backend is implemented
        return [];
      }
      console.error('courierService.getAll failed:', e);
      return [];
    }
  },

  /** @backend-needed GET /v1/np/couriers/{id} */
  async getById(id: number): Promise<Courier | undefined> {
    try {
      const { data } = await api.get<Courier>(`/v1/np/couriers/${id}`);
      return data;
    } catch (e) {
      console.error(`courierService.getById(${id}) failed:`, e);
      return undefined;
    }
  },

  /** @backend-needed GET /v1/np/couriers?status=active */
  async getActive(): Promise<Courier[]> {
    try {
      const { data } = await api.get<Courier[]>('/v1/np/couriers', { params: { status: 'active' } });
      return data ?? [];
    } catch (e: any) {
      if (e?.response?.status === 404 || e?.code === 'ERR_NETWORK') {
        return [];
      }
      console.error('courierService.getActive failed:', e);
      return [];
    }
  },

  /** @backend-needed GET /v1/np/couriers?type=Master */
  async getMasters(): Promise<Courier[]> {
    try {
      const { data } = await api.get<Courier[]>('/v1/np/couriers', { params: { type: 'Master' } });
      return data ?? [];
    } catch (e: any) {
      if (e?.response?.status === 404 || e?.code === 'ERR_NETWORK') {
        return [];
      }
      console.error('courierService.getMasters failed:', e);
      return [];
    }
  },

  /** @backend-needed GET /v1/np/couriers?masterId={masterId} */
  async getSubsForMaster(masterId: number): Promise<Courier[]> {
    try {
      const { data } = await api.get<Courier[]>('/v1/np/couriers', { params: { masterId } });
      return data ?? [];
    } catch (e: any) {
      if (e?.response?.status === 404 || e?.code === 'ERR_NETWORK') {
        return [];
      }
      console.error(`courierService.getSubsForMaster(${masterId}) failed:`, e);
      return [];
    }
  },

  /** @backend-needed GET /v1/np/couriers?search={query}&status=...&location=...&vehicle=... */
  async search(query: string, filters?: { status?: string; location?: string; vehicle?: string }): Promise<Courier[]> {
    try {
      const params: Record<string, string> = {};
      if (query) params.search = query;
      if (filters?.status && filters.status !== 'All Status') params.status = filters.status;
      if (filters?.location && filters.location !== 'All Locations') params.location = filters.location;
      if (filters?.vehicle && filters.vehicle !== 'All Vehicles') params.vehicle = filters.vehicle;
      const { data } = await api.get<Courier[]>('/v1/np/couriers', { params });
      return data ?? [];
    } catch (e: any) {
      if (e?.response?.status === 404 || e?.code === 'ERR_NETWORK') {
        return [];
      }
      console.error('courierService.search failed:', e);
      return [];
    }
  },

  /** @backend-needed GET /v1/np/couriers/portal-links */
  async getPortalLinks(): Promise<{ courierId: number; code: string; name: string; url: string }[]> {
    try {
      const { data } = await api.get<{ courierId: number; code: string; name: string; url: string }[]>('/v1/np/couriers/portal-links');
      return data ?? [];
    } catch (e: any) {
      if (e?.response?.status === 404 || e?.code === 'ERR_NETWORK') {
        return [];
      }
      console.error('courierService.getPortalLinks failed:', e);
      return [];
    }
  },
};
