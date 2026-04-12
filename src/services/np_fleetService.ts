/** @backend Loc: register this service in Program.cs */
import api from './np_api';

// ─── Types ─────────────────────────────────────────────────────────────────

export interface Fleet {
  id: number;
  name: string;
  depotId: number | null;
  directCostAccountCode: string;
  notes: string;
  displayOnClearlistsDespatch: boolean;
  displayOnClearlistsDevice: boolean;
  allowCourierPortalAccess: boolean;
  allowInvoicing: boolean;
  allowSchedules: boolean;
  created: string;
  createdBy: string;
  lastModified: string;
  lastModifiedBy: string;
}

export interface FleetCourier {
  id: number;
  name: string;
  phone: string;
  status: 'active' | 'inactive';
  vehicle: string;
}

export interface Depot {
  id: number;
  name: string;
}

/**
 * Fleet group management — wired to FleetController.
 * Backend route: api/v1/np/fleet-groups
 */
export const fleetService = {
  /** @backend GET /api/v1/np/fleet-groups */
  async getAll(): Promise<Fleet[]> {
    try {
      const { data } = await api.get<Fleet[]>('/v1/np/fleet-groups');
      return data ?? [];
    } catch (e) {
      console.error('fleetService.getAll failed:', e);
      return [];
    }
  },

  /** @backend GET /api/v1/np/fleet-groups?search={query} */
  async search(query: string): Promise<Fleet[]> {
    try {
      const { data } = await api.get<Fleet[]>('/v1/np/fleet-groups', { params: { search: query } });
      return data ?? [];
    } catch (e) {
      console.error('fleetService.search failed:', e);
      return [];
    }
  },

  /** @backend GET /api/v1/np/fleet-groups/{id} */
  async getById(id: number): Promise<Fleet | undefined> {
    try {
      const { data } = await api.get<Fleet>(`/v1/np/fleet-groups/${id}`);
      return data;
    } catch (e) {
      console.error(`fleetService.getById(${id}) failed:`, e);
      return undefined;
    }
  },

  /** @backend POST /api/v1/np/fleet-groups */
  async create(dto: Omit<Fleet, 'id' | 'created' | 'createdBy' | 'lastModified' | 'lastModifiedBy'>): Promise<Fleet | undefined> {
    try {
      const { data } = await api.post<Fleet>('/v1/np/fleet-groups', dto);
      return data;
    } catch (e) {
      console.error('fleetService.create failed:', e);
      return undefined;
    }
  },

  /** @backend PUT /api/v1/np/fleet-groups/{id} */
  async update(id: number, dto: Partial<Fleet>): Promise<Fleet | undefined> {
    try {
      const { data } = await api.put<Fleet>(`/v1/np/fleet-groups/${id}`, dto);
      return data;
    } catch (e) {
      console.error(`fleetService.update(${id}) failed:`, e);
      return undefined;
    }
  },

  /** @backend DELETE /api/v1/np/fleet-groups/{id} */
  async delete(id: number): Promise<boolean> {
    try {
      await api.delete(`/v1/np/fleet-groups/${id}`);
      return true;
    } catch (e) {
      console.error(`fleetService.delete(${id}) failed:`, e);
      return false;
    }
  },

  /** @backend GET /api/v1/np/fleet-groups/{id}/couriers */
  async getCouriers(fleetId: number): Promise<FleetCourier[]> {
    try {
      const { data } = await api.get<FleetCourier[]>(`/v1/np/fleet-groups/${fleetId}/couriers`);
      return data ?? [];
    } catch (e) {
      console.error(`fleetService.getCouriers(${fleetId}) failed:`, e);
      return [];
    }
  },

  /** @backend POST /api/v1/np/fleet-groups/{id}/assign */
  async assignCourier(fleetId: number, courierId: number): Promise<boolean> {
    try {
      await api.post(`/v1/np/fleet-groups/${fleetId}/assign`, { courierId });
      return true;
    } catch (e) {
      console.error(`fleetService.assignCourier(${fleetId}, ${courierId}) failed:`, e);
      return false;
    }
  },

  /** @backend GET /api/v1/np/settings — depots come from settings */
  async getDepots(): Promise<Depot[]> {
    try {
      const { data } = await api.get<{ depots: Depot[] }>('/v1/np/settings');
      return data?.depots ?? [];
    } catch (e) {
      console.error('fleetService.getDepots failed:', e);
      return [];
    }
  },

  getDepotName(depots: Depot[], depotId: number | null): string {
    if (!depotId) return '—';
    return depots.find(d => d.id === depotId)?.name ?? '—';
  },
};
