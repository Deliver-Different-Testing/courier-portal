// TODO: Implement document service — stubs for build

export interface DocumentType {
  id: number;
  name: string;
  category?: string;
  description?: string;
  isRequired: boolean;
  isActive: boolean;
  contentUrl?: string;
  instructions?: string;
  estimatedMinutes?: number;
}

export interface CourierDocument {
  id: number;
  courierId: number;
  documentTypeId: number;
  fileName: string;
  status: string;
  uploadedDate: string;
}

export const documentTypeService = {
  async getAll(): Promise<DocumentType[]> {
    const res = await fetch('/api/portal-steps');
    return res.ok ? res.json() : [];
  },
  async create(_data: Partial<DocumentType>) {
    return null;
  },
  async update(_id: number, _data: Partial<DocumentType>) {
    return null;
  },
  async delete(_id: number) {
    return { success: true };
  },
};

export const courierDocumentService = {
  async getByCourier(_courierId: number): Promise<CourierDocument[]> {
    return [];
  },
  async upload(_courierId: number, _docTypeId: number, _file: File) {
    return { success: true };
  },
};
