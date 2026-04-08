// Fleet Management Service — mock data for TucCourierFleet

export interface Fleet {
  id: number;
  name: string;
  depotId: number | null;
  directCostAccountCode: string;
  notes: string;
  displayOnClearlistsDespatch: boolean;
  displayOnClearlistsDevice: boolean;
  allowCourierPortalAccess: boolean;
  allowInvoicing: boolean;
  allowSchedules: boolean;
  created: string;
  createdBy: string;
  lastModified: string;
  lastModifiedBy: string;
}

export interface FleetCourier {
  id: number;
  name: string;
  phone: string;
  status: 'active' | 'inactive';
  vehicle: string;
}

export interface Depot {
  id: number;
  name: string;
}

const depots: Depot[] = [
  { id: 1, name: 'Chicago' },
  { id: 2, name: 'Dallas' },
  { id: 3, name: 'Houston' },
  { id: 4, name: 'Los Angeles' },
  { id: 5, name: 'Miami' },
];

let fleets: Fleet[] = [
  {
    id: 1,
    name: 'Chicago Fleet',
    depotId: 1,
    directCostAccountCode: 'CHI-DC-001',
    notes: 'Chicago metro delivery fleet. Covers Loop, West Side, and North Shore zones.',
    displayOnClearlistsDespatch: true,
    displayOnClearlistsDevice: true,
    allowCourierPortalAccess: true,
    allowInvoicing: true,
    allowSchedules: true,
    created: '2024-01-15T09:00:00Z',
    createdBy: 'admin@partner.com',
    lastModified: '2025-11-20T14:30:00Z',
    lastModifiedBy: 'admin@partner.com',
  },
  {
    id: 2,
    name: 'Dallas Fleet',
    depotId: 2,
    directCostAccountCode: 'DAL-DC-002',
    notes: 'Dallas-Fort Worth metro area fleet. Same-day and next-day deliveries.',
    displayOnClearlistsDespatch: true,
    displayOnClearlistsDevice: false,
    allowCourierPortalAccess: true,
    allowInvoicing: true,
    allowSchedules: false,
    created: '2024-03-10T11:00:00Z',
    createdBy: 'admin@partner.com',
    lastModified: '2025-10-05T09:15:00Z',
    lastModifiedBy: 'ops@partner.com',
  },
  {
    id: 3,
    name: 'Houston Fleet',
    depotId: 3,
    directCostAccountCode: 'HOU-DC-003',
    notes: 'Houston metro fleet. Midtown, Galleria, and surrounding areas.',
    displayOnClearlistsDespatch: true,
    displayOnClearlistsDevice: true,
    allowCourierPortalAccess: true,
    allowInvoicing: false,
    allowSchedules: true,
    created: '2024-06-01T08:00:00Z',
    createdBy: 'ops@partner.com',
    lastModified: '2025-12-01T16:45:00Z',
    lastModifiedBy: 'admin@partner.com',
  },
  {
    id: 4,
    name: 'Owner-Operators',
    depotId: null,
    directCostAccountCode: 'OO-DC-004',
    notes: 'Independent owner-operator couriers. No fixed depot. Contracted on per-job basis.',
    displayOnClearlistsDespatch: false,
    displayOnClearlistsDevice: false,
    allowCourierPortalAccess: true,
    allowInvoicing: true,
    allowSchedules: false,
    created: '2024-02-20T10:30:00Z',
    createdBy: 'admin@partner.com',
    lastModified: '2025-09-15T11:00:00Z',
    lastModifiedBy: 'admin@partner.com',
  },
];

const fleetCouriers: Record<number, FleetCourier[]> = {
  1: [
    { id: 101, name: 'James Wilson', phone: '+64 21 555 0101', status: 'active', vehicle: 'Van' },
    { id: 102, name: 'Sarah Chen', phone: '+64 21 555 0102', status: 'active', vehicle: 'Car' },
    { id: 103, name: 'Matiu Henare', phone: '+64 21 555 0103', status: 'active', vehicle: 'Van' },
    { id: 104, name: 'Lisa Patel', phone: '+64 21 555 0104', status: 'inactive', vehicle: 'Bike' },
    { id: 105, name: 'Tane Roberts', phone: '+64 21 555 0105', status: 'active', vehicle: 'Truck' },
  ],
  2: [
    { id: 201, name: 'Emma Taylor', phone: '+64 22 555 0201', status: 'active', vehicle: 'Van' },
    { id: 202, name: 'Ravi Kumar', phone: '+64 22 555 0202', status: 'active', vehicle: 'Car' },
    { id: 203, name: 'Ngaire Brown', phone: '+64 22 555 0203', status: 'active', vehicle: 'Van' },
  ],
  3: [
    { id: 301, name: 'Mike Johnson', phone: '+1 214 555 0301', status: 'active', vehicle: 'Van' },
    { id: 302, name: 'Carlos Garcia', phone: '+1 214 555 0302', status: 'active', vehicle: 'Truck' },
    { id: 303, name: 'Amy Davis', phone: '+1 214 555 0303', status: 'inactive', vehicle: 'Car' },
    { id: 304, name: 'Tyler Brooks', phone: '+1 214 555 0304', status: 'active', vehicle: 'Van' },
  ],
  4: [
    { id: 401, name: 'Dave Thompson', phone: '+64 27 555 0401', status: 'active', vehicle: 'Van' },
    { id: 402, name: 'Kim Nguyen', phone: '+64 27 555 0402', status: 'active', vehicle: 'Car' },
  ],
};

let nextId = 5;

export const fleetService = {
  getAll(): Fleet[] {
    return [...fleets];
  },

  search(query: string): Fleet[] {
    if (!query.trim()) return [...fleets];
    const q = query.toLowerCase();
    return fleets.filter(
      (f) =>
        f.name.toLowerCase().includes(q) ||
        f.directCostAccountCode.toLowerCase().includes(q) ||
        (f.notes && f.notes.toLowerCase().includes(q))
    );
  },

  getById(id: number): Fleet | undefined {
    return fleets.find((f) => f.id === id);
  },

  create(data: Omit<Fleet, 'id' | 'created' | 'createdBy' | 'lastModified' | 'lastModifiedBy'>): Fleet {
    const now = new Date().toISOString();
    const fleet: Fleet = {
      ...data,
      id: nextId++,
      created: now,
      createdBy: 'admin@partner.com',
      lastModified: now,
      lastModifiedBy: 'admin@partner.com',
    };
    fleets.push(fleet);
    fleetCouriers[fleet.id] = [];
    return fleet;
  },

  update(id: number, data: Partial<Fleet>): Fleet | undefined {
    const idx = fleets.findIndex((f) => f.id === id);
    if (idx === -1) return undefined;
    fleets[idx] = {
      ...fleets[idx],
      ...data,
      id,
      lastModified: new Date().toISOString(),
      lastModifiedBy: 'admin@partner.com',
    };
    return fleets[idx];
  },

  delete(id: number): boolean {
    const idx = fleets.findIndex((f) => f.id === id);
    if (idx === -1) return false;
    fleets.splice(idx, 1);
    delete fleetCouriers[id];
    return true;
  },

  getCouriers(fleetId: number): FleetCourier[] {
    return fleetCouriers[fleetId] || [];
  },

  getDepots(): Depot[] {
    return [...depots];
  },

  getDepotName(depotId: number | null): string {
    if (!depotId) return '—';
    return depots.find((d) => d.id === depotId)?.name || '—';
  },
};
