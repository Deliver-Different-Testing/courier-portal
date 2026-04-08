import api from './np_api';
import type { CourierContract } from '@/types';

export const contractService = {
  async getContracts(): Promise<CourierContract[]> {
    try {
      const { data } = await api.get<CourierContract[]>('/contracts');
      return data;
    } catch (e) {
      console.error('contractService.getContracts failed:', e);
      return [];
    }
  },

  async activateContract(id: number): Promise<CourierContract | undefined> {
    try {
      const { data } = await api.put<CourierContract>(`/contracts/${id}/activate`);
      return data;
    } catch (e) {
      console.error('contractService.activateContract failed:', e);
      return undefined;
    }
  },

  async deleteContract(id: number): Promise<boolean> {
    try {
      await api.delete(`/contracts/${id}`);
      return true;
    } catch (e) {
      console.error('contractService.deleteContract failed:', e);
      return false;
    }
  },
};
