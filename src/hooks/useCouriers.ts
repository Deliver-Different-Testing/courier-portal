import { useState, useEffect, useMemo } from 'react';
import { courierService } from '@/services/np_courierService';
import type { Courier } from '@/types';

export function useCouriers() {
  const [allCouriers, setAllCouriers] = useState<Courier[]>([]);
  const [search, setSearch] = useState('');
  const [statusFilter, setStatusFilter] = useState('All Status');
  const [locationFilter, setLocationFilter] = useState('All Locations');
  const [vehicleFilter, setVehicleFilter] = useState('All Vehicles');

  useEffect(() => {
    courierService.getAll().then(setAllCouriers).catch(() => setAllCouriers([]));
  }, []);

  const filtered = useMemo(() => {
    let result = allCouriers;
    if (search) result = result.filter(c =>
      `${c.firstName} ${c.surName} ${c.code}`.toLowerCase().includes(search.toLowerCase())
    );
    if (statusFilter !== 'All Status') result = result.filter(c => c.status === statusFilter.toLowerCase());
    if (locationFilter !== 'All Locations') result = result.filter(c => c.location === locationFilter);
    if (vehicleFilter !== 'All Vehicles') result = result.filter(c => c.vehicle === vehicleFilter);
    return result;
  }, [allCouriers, search, statusFilter, locationFilter, vehicleFilter]);

  const active = allCouriers.filter(c => c.status === 'active').length;
  const inactive = allCouriers.filter(c => c.status === 'inactive').length;
  const alerts = allCouriers.filter(c => (c as any).compliance !== 'ok').length;

  return {
    couriers: filtered,
    allCouriers,
    search, setSearch,
    statusFilter, setStatusFilter,
    locationFilter, setLocationFilter,
    vehicleFilter, setVehicleFilter,
    active, inactive, alerts,
  };
}
