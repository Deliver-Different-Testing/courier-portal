import { users } from './np_mockData';
import type { User } from '@/types';

export const userService = {
  getAll: (): User[] => users,
  getById: (id: string): User | undefined => users.find(u => u.id === id),
};
