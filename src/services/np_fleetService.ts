import api from './np_api';

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

export const fleetService = {
  async getAll(): Promise<Fleet[]> {
    try {
      const { data } = await api.get<Fleet[]>('/fleets');
      return data;
    } catch (e) {
      console.error('fleetService.getAll failed:', e);
      return [];
    }
  },

  async search(query: string): Promise<Fleet[]> {
    try {
      const { data } = await api.get<Fleet[]>('/fleets', { params: { q: query } });
      return data;
    } catch (e) {
      console.error('fleetService.search failed:', e);
      return [];
    }
  },

  async getById(id: number): Promise<Fleet | undefined> {
    try {
      const { data } = await api.get<Fleet>(`/fleets/${id}`);
      return data;
    } catch (e) {
      console.error('fleetService.getById failed:', e);
      return undefined;
    }
  },

  async create(data: Omit<Fleet, 'id' | 'created' | 'createdBy' | 'lastModified' | 'lastModifiedBy'>): Promise<Fleet> {
    const { data: created } = await api.post<Fleet>('/fleets', data);
    return created;
  },

  async update(id: number, updates: Partial<Fleet>): Promise<Fleet | undefined> {
    try {
      const { data } = await api.put<Fleet>(`/fleets/${id}`, updates);
      return data;
    } catch (e) {
      console.error('fleetService.update failed:', e);
      return undefined;
    }
  },

  async delete(id: number): Promise<boolean> {
    try {
      await api.delete(`/fleets/${id}`);
      return true;
    } catch (e) {
      console.error('fleetService.delete failed:', e);
      return false;
    }
  },

  async getCouriers(fleetId: number): Promise<FleetCourier[]> {
    try {
      const { data } = await api.get<FleetCourier[]>(`/fleets/${fleetId}/vehicles`);
      return data;
    } catch (e) {
      console.error('fleetService.getCouriers failed:', e);
      return [];
    }
  },

  async getDepots(): Promise<Depot[]> {
    try {
      const { data } = await api.get<Depot[]>('/fleets/depots');
      return data;
    } catch (e) {
      console.error('fleetService.getDepots failed:', e);
      return [];
    }
  },

  async getDepotName(depotId: number | null): Promise<string> {
    if (!depotId) return '—';
    try {
      const depots = await this.getDepots();
      return depots.find((d: Depot) => d.id === depotId)?.name || '—';
    } catch {
      return '—';
    }
  },
};
