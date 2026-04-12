export const settingsService = {
  getSettings: () => ({
    name: 'Pacific Express Logistics',
    code: 'PXL',
    address: '200 W Adams St, Suite 1500, Chicago, IL 60606',
    phone: '312-555-1000',
    email: 'admin@pacificexpress.com',
    coverageAreas: ['Chicago Downtown', 'Chicago West', 'Chicago North', 'Chicago Loop', 'Dallas Downtown', 'Dallas North', 'Dallas South', 'Dallas East', 'Houston Midtown', 'Houston Galleria', 'Houston Heights'],
    notifications: {
      'New job assignments': true,
      'Job completion alerts': true,
      'Compliance expiry warnings': true,
      'Daily summary email': false,
      'Weekly report email': true,
      'Courier status changes': true,
    },
  }),
};
