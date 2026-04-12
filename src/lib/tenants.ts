export interface DocFieldConfig {
  key: string;
  label: string;
  type: 'text' | 'date' | 'select';
  required?: boolean;
}

export interface TenantDocRequirement {
  id: number;
  name: string;
  mandatory: boolean;
  instructions: string;
  countriesOnly?: ('NZ' | 'US')[];
  rolesOnly?: string[];
  extractableFields?: DocFieldConfig[];
}

export interface TenantBrand {
  slug: string;
  name: string;
  logoUrl: string | null;
  primaryColor: string;
  accentColor: string;
  country: 'NZ' | 'US';
  tagline: string;
  /** Override default doc requirements — tenant controls what's mandatory */
  docRequirements?: TenantDocRequirement[];
  /** What to call insurance — defaults to 'Vehicle Insurance' */
  insuranceLabel?: string;
  insuranceDescription?: string;
}

// ── Default doc requirements (used when tenant doesn't override) ──

export const DEFAULT_DOC_REQUIREMENTS: TenantDocRequirement[] = [
  { id: 1, name: "Driver's License", mandatory: true, instructions: 'Upload a clear photo of front and back.', extractableFields: [
    { key: 'licenseNumber', label: 'License Number', type: 'text', required: true },
    { key: 'expiryDate', label: 'Expiry Date', type: 'date', required: true },
    { key: 'licenseClass', label: 'License Class', type: 'text' },
    { key: 'endorsements', label: 'Endorsements', type: 'text' },
  ]},
  { id: 2, name: 'Vehicle Registration', mandatory: true, instructions: 'Current registration document.', extractableFields: [
    { key: 'plateNumber', label: 'Plate Number', type: 'text', required: true },
    { key: 'regoExpiry', label: 'Registration Expiry', type: 'date', required: true },
  ]},
  { id: 3, name: 'Vehicle Insurance', mandatory: false, instructions: 'Basic vehicle insurance — third party or comprehensive.', extractableFields: [
    { key: 'policyNumber', label: 'Policy Number', type: 'text', required: true },
    { key: 'insurerName', label: 'Insurer Name', type: 'text', required: true },
    { key: 'coverageAmount', label: 'Coverage Amount', type: 'text' },
    { key: 'expiryDate', label: 'Expiry Date', type: 'date', required: true },
  ]},
  { id: 4, name: 'WOF Certificate', mandatory: true, instructions: 'Current Warrant of Fitness.', countriesOnly: ['NZ'], extractableFields: [
    { key: 'wofExpiry', label: 'WOF Expiry', type: 'date', required: true },
  ]},
  { id: 5, name: 'TSL Certificate', mandatory: false, instructions: 'Transport Service License (if applicable).', countriesOnly: ['NZ'], extractableFields: [
    { key: 'tslNumber', label: 'TSL Number', type: 'text' },
    { key: 'tslExpiry', label: 'TSL Expiry', type: 'date' },
  ]},
  { id: 6, name: 'DOT Registration', mandatory: true, instructions: 'Department of Transportation registration.', countriesOnly: ['US'], extractableFields: [
    { key: 'dotNumber', label: 'DOT Number', type: 'text', required: true },
    { key: 'expiryDate', label: 'Expiry Date', type: 'date', required: true },
  ]},
  { id: 7, name: 'Dangerous Goods Certificate', mandatory: true, instructions: 'Required for DG and medical courier roles.', rolesOnly: ['medical', 'dg'], extractableFields: [
    { key: 'dgCertNumber', label: 'Certificate Number', type: 'text' },
    { key: 'dgExpiry', label: 'Expiry Date', type: 'date', required: true },
  ]},
];

const TENANTS: Record<string, TenantBrand> = {
  'urgent-couriers': {
    slug: 'urgent-couriers',
    name: 'Urgent Couriers',
    logoUrl: '/NP-Agent-Management/portal/logos/urgent-logo.png',
    primaryColor: '#1a1a1a',
    accentColor: '#f5c518',
    country: 'NZ',
    tagline: 'Fast. Reliable. Kiwi-owned.',
    insuranceLabel: 'Vehicle Insurance',
    insuranceDescription: 'Basic third-party vehicle insurance minimum.',
    // Uses default docs — insurance NOT mandatory for general couriers
  },
  'medical-express': {
    slug: 'medical-express',
    name: 'Medical Couriers',
    logoUrl: '/NP-Agent-Management/portal/logos/medical-couriers.jpg',
    primaryColor: '#1a365d',
    accentColor: '#2b6cb0',
    country: 'US',
    tagline: 'Trusted healthcare logistics.',
    insuranceLabel: 'Liability Insurance',
    insuranceDescription: 'Proof of commercial auto liability coverage ($1M minimum).',
    // Medical tenant overrides: insurance IS mandatory, higher standard
    docRequirements: [
      { id: 1, name: "Driver's License", mandatory: true, instructions: 'Upload a clear photo of front and back.', extractableFields: [
        { key: 'licenseNumber', label: 'License Number', type: 'text', required: true },
        { key: 'expiryDate', label: 'Expiry Date', type: 'date', required: true },
        { key: 'licenseClass', label: 'License Class', type: 'text' },
        { key: 'endorsements', label: 'Endorsements', type: 'text' },
      ]},
      { id: 2, name: 'Vehicle Registration', mandatory: true, instructions: 'Current registration document.', extractableFields: [
        { key: 'plateNumber', label: 'Plate Number', type: 'text', required: true },
        { key: 'regoExpiry', label: 'Registration Expiry', type: 'date', required: true },
      ]},
      { id: 3, name: 'Commercial Auto Insurance', mandatory: true, instructions: 'Proof of commercial auto liability coverage ($1M minimum).', extractableFields: [
        { key: 'policyNumber', label: 'Policy Number', type: 'text', required: true },
        { key: 'insurerName', label: 'Insurer Name', type: 'text', required: true },
        { key: 'coverageAmount', label: 'Coverage Amount', type: 'text' },
        { key: 'expiryDate', label: 'Expiry Date', type: 'date', required: true },
      ]},
      { id: 6, name: 'DOT Registration', mandatory: true, instructions: 'Department of Transportation registration.', countriesOnly: ['US'], extractableFields: [
        { key: 'dotNumber', label: 'DOT Number', type: 'text', required: true },
        { key: 'expiryDate', label: 'Expiry Date', type: 'date', required: true },
      ]},
      { id: 7, name: 'Dangerous Goods Certificate', mandatory: true, instructions: 'Required for all medical courier roles.', rolesOnly: ['medical', 'dg'], extractableFields: [
        { key: 'dgCertNumber', label: 'Certificate Number', type: 'text' },
        { key: 'dgExpiry', label: 'Expiry Date', type: 'date', required: true },
      ]},
      { id: 8, name: 'HIPAA Training Certificate', mandatory: true, instructions: 'Proof of HIPAA compliance training.', rolesOnly: ['medical'], extractableFields: [
        { key: 'certificateNumber', label: 'Certificate Number', type: 'text' },
        { key: 'completionDate', label: 'Completion Date', type: 'date', required: true },
      ]},
    ],
  },
  'dfrnt': {
    slug: 'dfrnt',
    name: 'Deliver Different',
    logoUrl: '/NP-Agent-Management/portal/logos/dfrnt-logo-white.png',
    primaryColor: '#0d0c2c',
    accentColor: '#3bc7f4',
    country: 'NZ',
    tagline: 'Delivering differently.',
  },
};

const DEFAULT_TENANT: TenantBrand = {
  slug: 'default',
  name: 'Courier Application',
  logoUrl: null,
  primaryColor: '#0d0c2c',
  accentColor: '#3bc7f4',
  country: 'US',
  tagline: 'Apply to become a courier driver.',
};

export function resolveTenant(slug?: string): TenantBrand {
  if (!slug) return DEFAULT_TENANT;
  return TENANTS[slug] || { ...DEFAULT_TENANT, slug, name: slug.replace(/-/g, ' ').replace(/\b\w/g, c => c.toUpperCase()) };
}
