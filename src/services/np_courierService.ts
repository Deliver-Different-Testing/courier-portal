import api from './np_api';
import type { Courier } from '@/types';

export const courierService = {
  async getAll(): Promise<Courier[]> {
    try {
      const { data } = await api.get<Courier[]>('/couriers');
      return data;
    } catch (e) {
      console.error('courierService.getAll failed:', e);
      return [];
    }
  },

  async getById(id: number): Promise<Courier | undefined> {
    try {
      const { data } = await api.get<Courier>(`/couriers/${id}`);
      return data;
    } catch (e) {
      console.error('courierService.getById failed:', e);
      return undefined;
    }
  },

  async getActive(): Promise<Courier[]> {
    try {
      const { data } = await api.get<Courier[]>('/couriers', { params: { status: 'active' } });
      return data;
    } catch (e) {
      console.error('courierService.getActive failed:', e);
      return [];
    }
  },

  async getMasters(): Promise<Courier[]> {
    try {
      const { data } = await api.get<Courier[]>('/couriers', { params: { type: 'Master' } });
      return data;
    } catch (e) {
      console.error('courierService.getMasters failed:', e);
      return [];
    }
  },

  async getSubsForMaster(masterId: number): Promise<Courier[]> {
    try {
      const { data } = await api.get<Courier[]>(`/couriers`, { params: { masterId } });
      return data;
    } catch (e) {
      console.error('courierService.getSubsForMaster failed:', e);
      return [];
    }
  },

  async search(query: string, filters?: { status?: string; location?: string; vehicle?: string }): Promise<Courier[]> {
    try {
      const params: Record<string, string> = {};
      if (query) params.q = query;
      if (filters?.status && filters.status !== 'All Status') params.status = filters.status;
      if (filters?.location && filters.location !== 'All Locations') params.location = filters.location;
      if (filters?.vehicle && filters.vehicle !== 'All Vehicles') params.vehicle = filters.vehicle;
      const { data } = await api.get<Courier[]>('/couriers/search', { params });
      return data;
    } catch (e) {
      console.error('courierService.search failed:', e);
      return [];
    }
  },

  async getPortalLinks(): Promise<{ courierId: number; code: string; name: string; url: string }[]> {
    try {
      const { data } = await api.get<{ courierId: number; code: string; name: string; url: string }[]>('/couriers/portal-links');
      return data;
    } catch (e) {
      console.error('courierService.getPortalLinks failed:', e);
      return [];
    }
  },
};
