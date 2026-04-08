import { useState, useMemo } from 'react';
import { courierService } from '@/services/np_courierService';

export function useCouriers() {
  const [search, setSearch] = useState('');
  const [statusFilter, setStatusFilter] = useState('All Status');
  const [locationFilter, setLocationFilter] = useState('All Locations');
  const [vehicleFilter, setVehicleFilter] = useState('All Vehicles');

  const allCouriers = courierService.getAll();

  const filtered = useMemo(() => {
    return courierService.search(search, {
      status: statusFilter,
      location: locationFilter,
      vehicle: vehicleFilter,
    });
  }, [search, statusFilter, locationFilter, vehicleFilter]);

  const active = allCouriers.filter(c => c.status === 'active').length;
  const inactive = allCouriers.filter(c => c.status === 'inactive').length;
  const alerts = allCouriers.filter(c => c.compliance !== 'ok').length;

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
