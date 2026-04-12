import api from './np_api';
import type { User } from '@/types';

/**
 * @backend-needed GET /v1/np/users — Loc: build this endpoint
 * @backend-needed POST /v1/np/users — Loc: build this endpoint
 * @backend-needed DELETE /v1/np/users/{id} — Loc: build this endpoint
 */
export const userService = {
  /** @backend-needed GET /v1/np/users */
  async getAll(): Promise<User[]> {
    try {
      const { data } = await api.get<User[]>('/v1/np/users');
      return data ?? [];
    } catch (e: any) {
      if (e?.response?.status === 404 || e?.code === 'ERR_NETWORK') {
        // FALLBACK: returns empty array until backend is implemented
        return [];
      }
      console.error('userService.getAll failed:', e);
      return [];
    }
  },

  /** @backend-needed GET /v1/np/users/{id} */
  async getById(id: string): Promise<User | undefined> {
    try {
      const { data } = await api.get<User>(`/v1/np/users/${id}`);
      return data;
    } catch (e) {
      console.error(`userService.getById(${id}) failed:`, e);
      return undefined;
    }
  },

  /** @backend-needed POST /v1/np/users */
  async create(user: Partial<User>): Promise<User> {
    const { data } = await api.post<User>('/v1/np/users', user);
    return data;
  },

  /** @backend-needed DELETE /v1/np/users/{id} */
  async deleteUser(id: string): Promise<boolean> {
    try {
      await api.delete(`/v1/np/users/${id}`);
      return true;
    } catch (e) {
      console.error(`userService.deleteUser(${id}) failed:`, e);
      return false;
    }
  },
};
