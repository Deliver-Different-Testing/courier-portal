import type { CourierContract } from '@/types';
import { mockContracts } from './np_mockData';

export const contractService = {
  getContracts: (): CourierContract[] => mockContracts,

  activateContract: (id: number): CourierContract | undefined => {
    mockContracts.forEach(c => { c.isActive = c.id === id; });
    return mockContracts.find(c => c.id === id);
  },

  deleteContract: (id: number): boolean => {
    const idx = mockContracts.findIndex(c => c.id === id);
    if (idx < 0) return false;
    mockContracts.splice(idx, 1);
    return true;
  },
};
