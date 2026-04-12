import { useState, useEffect } from 'react';
import { userService } from '@/services/np_userService';
import type { User } from '@/types';

export function useUsers() {
  const [users, setUsers] = useState<User[]>([]);

  useEffect(() => {
    userService.getAll().then(setUsers).catch(() => setUsers([]));
  }, []);

  return { users };
}
