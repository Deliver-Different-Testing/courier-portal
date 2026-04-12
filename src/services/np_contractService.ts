import api from './np_api';
import type { CourierContract } from '@/types';

/**
 * @backend-needed GET /v1/np/contracts — Loc: build this endpoint
 * @backend-needed POST /v1/np/contracts — Loc: build this endpoint
 * @backend-needed PUT /v1/np/contracts/{id} — Loc: build this endpoint
 * @backend-needed DELETE /v1/np/contracts/{id} — Loc: build this endpoint
 */
export const contractService = {
  /** @backend-needed GET /v1/np/contracts */
  async getContracts(): Promise<CourierContract[]> {
    try {
      const { data } = await api.get<CourierContract[]>('/v1/np/contracts');
      return data ?? [];
    } catch (e: any) {
      if (e?.response?.status === 404 || e?.code === 'ERR_NETWORK') {
        // FALLBACK: returns empty array until backend is implemented
        return [];
      }
      console.error('contractService.getContracts failed:', e);
      return [];
    }
  },

  /** @backend-needed PUT /v1/np/contracts/{id}/activate */
  async activateContract(id: number): Promise<CourierContract | undefined> {
    try {
      const { data } = await api.put<CourierContract>(`/v1/np/contracts/${id}/activate`, {});
      return data;
    } catch (e) {
      console.error(`contractService.activateContract(${id}) failed:`, e);
      return undefined;
    }
  },

  /** @backend-needed DELETE /v1/np/contracts/{id} */
  async deleteContract(id: number): Promise<boolean> {
    try {
      await api.delete(`/v1/np/contracts/${id}`);
      return true;
    } catch (e) {
      console.error(`contractService.deleteContract(${id}) failed:`, e);
      return false;
    }
  },
};
