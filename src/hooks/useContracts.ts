import { useState, useEffect, useCallback } from 'react';
import { contractService } from '@/services/np_contractService';
import type { CourierContract } from '@/types';

export function useContracts() {
  const [contracts, setContracts] = useState<CourierContract[]>([]);
  const [refreshKey, setRefreshKey] = useState(0);
  const refresh = useCallback(() => setRefreshKey(k => k + 1), []);

  useEffect(() => {
    contractService.getContracts().then(setContracts).catch(() => setContracts([]));
  }, [refreshKey]);

  const activateContract = useCallback(async (id: number) => {
    await contractService.activateContract(id);
    refresh();
  }, [refresh]);

  const deleteContract = useCallback(async (id: number) => {
    await contractService.deleteContract(id);
    refresh();
  }, [refresh]);

  return { contracts, activateContract, deleteContract, refresh };
}
