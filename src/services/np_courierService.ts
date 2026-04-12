import { couriers } from './np_mockData';
import type { Courier } from '@/types';

export const courierService = {
  getAll: (): Courier[] => couriers,

  getById: (id: number): Courier | undefined => couriers.find(c => c.id === id),

  getActive: (): Courier[] => couriers.filter(c => c.status === 'active'),

  getMasters: (): Courier[] => couriers.filter(c => c.type === 'Master'),

  getSubsForMaster: (masterId: number): Courier[] => couriers.filter(c => c.master === masterId),

  search: (query: string, filters?: { status?: string; location?: string; vehicle?: string }): Courier[] => {
    let result = [...couriers];
    if (query) {
      const q = query.toLowerCase();
      result = result.filter(c =>
        `${c.firstName} ${c.surName} ${c.code}`.toLowerCase().includes(q)
      );
    }
    if (filters?.status && filters.status !== 'All Status') {
      result = result.filter(c => c.status === filters.status!.toLowerCase());
    }
    if (filters?.location && filters.location !== 'All Locations') {
      result = result.filter(c => c.location === filters.location);
    }
    if (filters?.vehicle && filters.vehicle !== 'All Vehicles') {
      result = result.filter(c => c.vehicle === filters.vehicle);
    }
    return result;
  },

  getPortalLinks: () => {
    return couriers
      .filter(c => c.status === 'active')
      .map(c => ({
        courierId: c.id,
        code: c.code,
        name: `${c.firstName} ${c.surName}`,
        url: `https://portal.dfrnt.com/courier/${c.code.toLowerCase()}/${Math.random().toString(36).substr(2, 12)}`,
      }));
  },
};
