import type {
  DocumentType,
  CourierDocument,
  DocumentUploadResult,
  DocumentExtractionResult,
  DocumentCategory,
  DocumentAppliesTo,
  DocumentPurpose,
} from '@/types';

// ─── localStorage-backed mock for static demo ───

const STORAGE_KEY = 'np_document_types_v4';

const DEFAULT_SEED: DocumentType[] = [
  { id: 1, name: "Driver's License", instructions: 'Upload a clear photo of front and back', category: 'Licensing', mandatory: true, active: true, hasExpiry: true, expiryWarningDays: 30, blockOnExpiry: true, appliesTo: 'Both', purpose: 'Compliance', sortOrder: 1, quizRequired: false, hasTemplate: false },
  { id: 2, name: 'Vehicle Registration', instructions: 'Current vehicle registration certificate', category: 'Vehicle', mandatory: true, active: true, hasExpiry: true, expiryWarningDays: 30, blockOnExpiry: true, appliesTo: 'Both', purpose: 'Compliance', sortOrder: 2, quizRequired: false, hasTemplate: false },
  { id: 3, name: 'Insurance Certificate', instructions: 'Comprehensive or third-party vehicle insurance', category: 'Insurance', mandatory: false, active: true, hasExpiry: true, expiryWarningDays: 30, blockOnExpiry: false, appliesTo: 'Both', purpose: 'Compliance', sortOrder: 3, quizRequired: false, hasTemplate: false },
  { id: 4, name: 'WOF Certificate', instructions: 'Warrant of Fitness (NZ only)', category: 'Vehicle', mandatory: true, active: true, hasExpiry: true, expiryWarningDays: 14, blockOnExpiry: true, appliesTo: 'Both', purpose: 'Compliance', sortOrder: 4, quizRequired: false, hasTemplate: false },
  { id: 5, name: 'DG Certificate', instructions: 'Dangerous Goods handling certificate (medical/DG roles only)', category: 'Licensing', mandatory: false, active: true, hasExpiry: true, expiryWarningDays: 60, blockOnExpiry: true, appliesTo: 'Both', purpose: 'Compliance', sortOrder: 5, quizRequired: false, hasTemplate: false },
  { id: 6, name: 'Contractor Agreement', instructions: 'Signed independent contractor agreement', category: 'Contract', mandatory: true, active: true, hasExpiry: false, expiryWarningDays: 0, blockOnExpiry: false, appliesTo: 'Applicant', purpose: 'Compliance', sortOrder: 6, quizRequired: false, hasTemplate: true },
  { id: 7, name: 'Safety Induction Video', instructions: 'Watch the safety induction video and sign off', category: 'Other', mandatory: true, active: true, hasExpiry: false, expiryWarningDays: 0, blockOnExpiry: false, appliesTo: 'ActiveCourier', purpose: 'Training', sortOrder: 10, quizRequired: true, hasTemplate: false, contentUrl: 'https://example.com/safety-induction', estimatedMinutes: 15 },
  { id: 8, name: 'Company Policies & Procedures', instructions: 'Read and acknowledge company policies', category: 'Other', mandatory: true, active: true, hasExpiry: false, expiryWarningDays: 0, blockOnExpiry: false, appliesTo: 'ActiveCourier', purpose: 'Training', sortOrder: 11, quizRequired: true, hasTemplate: true, contentUrl: '', estimatedMinutes: 20 },
  { id: 9, name: 'Customer Service Standards', instructions: 'Review customer service expectations and best practices', category: 'Other', mandatory: false, active: true, hasExpiry: false, expiryWarningDays: 0, blockOnExpiry: false, appliesTo: 'ActiveCourier', purpose: 'Training', sortOrder: 12, quizRequired: false, hasTemplate: false, contentUrl: 'https://example.com/customer-service', estimatedMinutes: 10 },
  // NP Company-Level Documents
  { id: 10, name: 'Business Registration', instructions: 'Company registration / incorporation certificate', category: 'Licensing', mandatory: true, active: true, hasExpiry: false, expiryWarningDays: 0, blockOnExpiry: false, appliesTo: 'NP', purpose: 'Compliance', sortOrder: 20, quizRequired: false, hasTemplate: false },
  { id: 11, name: 'Proof of Insurance', instructions: 'General liability & commercial vehicle insurance', category: 'Insurance', mandatory: true, active: true, hasExpiry: true, expiryWarningDays: 30, blockOnExpiry: true, appliesTo: 'NP', purpose: 'Compliance', sortOrder: 21, quizRequired: false, hasTemplate: false },
  { id: 12, name: 'Workers Compensation Certificate', instructions: 'Current workers comp certificate of currency', category: 'Insurance', mandatory: true, active: true, hasExpiry: true, expiryWarningDays: 30, blockOnExpiry: true, appliesTo: 'NP', purpose: 'Compliance', sortOrder: 22, quizRequired: false, hasTemplate: false },
  { id: 13, name: 'Operating Authority', instructions: 'DOT / MC number or state operating authority', category: 'Licensing', mandatory: true, active: true, hasExpiry: true, expiryWarningDays: 60, blockOnExpiry: true, appliesTo: 'NP', purpose: 'Compliance', sortOrder: 23, quizRequired: false, hasTemplate: false },
  { id: 14, name: 'Service Agreement', instructions: 'Signed NP service agreement with tenant', category: 'Contract', mandatory: true, active: true, hasExpiry: false, expiryWarningDays: 0, blockOnExpiry: false, appliesTo: 'NP', purpose: 'Compliance', sortOrder: 24, quizRequired: false, hasTemplate: true },
  { id: 15, name: 'W-9 / Tax ID', instructions: 'Tax identification for payment processing', category: 'Other', mandatory: true, active: true, hasExpiry: false, expiryWarningDays: 0, blockOnExpiry: false, appliesTo: 'NP', purpose: 'Compliance', sortOrder: 25, quizRequired: false, hasTemplate: false },
  { id: 16, name: 'References', instructions: 'Business references from existing clients', category: 'Other', mandatory: false, active: true, hasExpiry: false, expiryWarningDays: 0, blockOnExpiry: false, appliesTo: 'NP', purpose: 'Compliance', sortOrder: 26, quizRequired: false, hasTemplate: false },
  { id: 17, name: 'Fleet Inventory', instructions: 'List of vehicles available for service', category: 'Vehicle', mandatory: false, active: true, hasExpiry: false, expiryWarningDays: 0, blockOnExpiry: false, appliesTo: 'NP', purpose: 'Compliance', sortOrder: 27, quizRequired: false, hasTemplate: false },
];

function loadTypes(): DocumentType[] {
  try {
    const raw = localStorage.getItem(STORAGE_KEY);
    if (raw) {
      const parsed = JSON.parse(raw);
      if (parsed.length > 0) return parsed;
    }
  } catch { /* ignore */ }
  // Seed defaults on first load
  saveTypes(DEFAULT_SEED);
  return [...DEFAULT_SEED];
}

function saveTypes(types: DocumentType[]): void {
  localStorage.setItem(STORAGE_KEY, JSON.stringify(types));
}

function nextId(types: DocumentType[]): number {
  return types.length > 0 ? Math.max(...types.map(t => t.id)) + 1 : 1;
}

// Simulate async
const delay = (ms = 50) => new Promise(r => setTimeout(r, ms));

export const documentTypeService = {
  async getAll(): Promise<DocumentType[]> {
    await delay();
    return loadTypes().filter(t => t.active);
  },

  async getById(id: number): Promise<DocumentType> {
    await delay();
    const found = loadTypes().find(t => t.id === id);
    if (!found) throw new Error('Not found');
    return found;
  },

  async create(dto: Partial<DocumentType>): Promise<DocumentType> {
    await delay();
    const types = loadTypes();
    const newType: DocumentType = {
      id: nextId(types),
      name: dto.name || '',
      instructions: dto.instructions || '',
      category: dto.category || 'Other',
      mandatory: dto.mandatory ?? false,
      active: true,
      hasExpiry: dto.hasExpiry ?? false,
      expiryWarningDays: dto.expiryWarningDays ?? 30,
      blockOnExpiry: dto.blockOnExpiry ?? false,
      appliesTo: dto.appliesTo || 'Both',
      purpose: dto.purpose || 'Compliance',
      sortOrder: dto.sortOrder ?? types.length + 1,
      quizRequired: dto.quizRequired ?? false,
      hasTemplate: false,
      contentUrl: dto.contentUrl,
      estimatedMinutes: dto.estimatedMinutes,
    };
    types.push(newType);
    saveTypes(types);
    return newType;
  },

  async update(id: number, dto: Partial<DocumentType>): Promise<DocumentType> {
    await delay();
    const types = loadTypes();
    const idx = types.findIndex(t => t.id === id);
    if (idx === -1) throw new Error('Not found');
    types[idx] = { ...types[idx], ...dto, id };
    saveTypes(types);
    return types[idx];
  },

  async deactivate(id: number): Promise<void> {
    await delay();
    const types = loadTypes();
    const idx = types.findIndex(t => t.id === id);
    if (idx !== -1) {
      types[idx].active = false;
      saveTypes(types);
    }
  },

  async downloadTemplate(_id: number): Promise<Blob> {
    return new Blob(['template placeholder'], { type: 'application/pdf' });
  },

  async uploadTemplate(id: number, _file: File): Promise<{ s3Key: string; fileName: string }> {
    await delay();
    const types = loadTypes();
    const idx = types.findIndex(t => t.id === id);
    if (idx !== -1) {
      types[idx].hasTemplate = true;
      saveTypes(types);
    }
    return { s3Key: `templates/${id}`, fileName: _file.name };
  },

  async seed(): Promise<DocumentType[]> {
    await delay();
    saveTypes(DEFAULT_SEED);
    return DEFAULT_SEED;
  },
};

// --- Courier Document CRUD (mock) ---

export const courierDocumentService = {
  async getDocuments(_courierId: number): Promise<CourierDocument[]> {
    await delay();
    return [];
  },

  async upload(
    _courierId: number,
    _documentTypeId: number,
    _file: File
  ): Promise<DocumentUploadResult> {
    await delay();
    return { documentId: Date.now(), fileName: _file.name, status: 'Current', extraction: null };
  },

  async getDownloadUrl(_courierId: number, _docId: number): Promise<string> {
    return '#';
  },

  async delete(_courierId: number, _docId: number): Promise<void> {
    await delay();
  },

  async verify(_courierId: number, _docId: number): Promise<CourierDocument> {
    await delay();
    return {} as CourierDocument;
  },

  async extractOnly(_courierId: number, _file: File): Promise<DocumentExtractionResult> {
    await delay();
    return { detectedDocumentType: null, overallConfidence: 0.95, fields: [], detectedExpiryDate: null, autoAccepted: false };
  },
};
