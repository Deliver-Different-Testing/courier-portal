import api from './np_api';
import type { User } from '@/types';

export const userService = {
  async getAll(): Promise<User[]> {
    try {
      const { data } = await api.get<User[]>('/users');
      return data;
    } catch (e) {
      console.error('userService.getAll failed:', e);
      return [];
    }
  },

  async getById(id: string): Promise<User | undefined> {
    try {
      const { data } = await api.get<User>(`/users/${id}`);
      return data;
    } catch (e) {
      console.error('userService.getById failed:', e);
      return undefined;
    }
  },

  async create(userData: Partial<User>): Promise<User> {
    const { data } = await api.post<User>('/users', userData);
    return data;
  },

  async update(id: string, userData: Partial<User>): Promise<User | undefined> {
    try {
      const { data } = await api.put<User>(`/users/${id}`, userData);
      return data;
    } catch (e) {
      console.error('userService.update failed:', e);
      return undefined;
    }
  },

  async delete(id: string): Promise<boolean> {
    try {
      await api.delete(`/users/${id}`);
      return true;
    } catch (e) {
      console.error('userService.delete failed:', e);
      return false;
    }
  },
};
