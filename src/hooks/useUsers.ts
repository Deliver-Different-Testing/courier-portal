import { userService } from '@/services/np_userService';

export function useUsers() {
  const users = userService.getAll();
  return { users };
}
