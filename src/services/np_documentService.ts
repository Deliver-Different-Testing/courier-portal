import api from './np_api';
import type {
  DocumentType,
  CourierDocument,
  DocumentUploadResult,
  DocumentExtractionResult,
} from '@/types';

export const documentTypeService = {
  async getAll(): Promise<DocumentType[]> {
    try {
      const { data } = await api.get<DocumentType[]>('/document-types');
      return data;
    } catch (e) {
      console.error('documentTypeService.getAll failed:', e);
      return [];
    }
  },

  async getById(id: number): Promise<DocumentType> {
    const { data } = await api.get<DocumentType>(`/document-types/${id}`);
    return data;
  },

  async create(dto: Partial<DocumentType>): Promise<DocumentType> {
    const { data } = await api.post<DocumentType>('/document-types', dto);
    return data;
  },

  async update(id: number, dto: Partial<DocumentType>): Promise<DocumentType> {
    const { data } = await api.put<DocumentType>(`/document-types/${id}`, dto);
    return data;
  },

  async deactivate(id: number): Promise<void> {
    await api.put(`/document-types/${id}/deactivate`);
  },

  async downloadTemplate(id: number): Promise<Blob> {
    const { data } = await api.get(`/document-types/${id}/template`, { responseType: 'blob' });
    return data;
  },

  async uploadTemplate(id: number, file: File): Promise<{ s3Key: string; fileName: string }> {
    const form = new FormData();
    form.append('file', file);
    const { data } = await api.post<{ s3Key: string; fileName: string }>(
      `/document-types/${id}/template`,
      form,
      { headers: { 'Content-Type': 'multipart/form-data' } }
    );
    return data;
  },

  async seed(): Promise<DocumentType[]> {
    const { data } = await api.post<DocumentType[]>('/document-types/seed');
    return data;
  },
};

// --- Courier Document CRUD ---

export const courierDocumentService = {
  async getDocuments(courierId: number): Promise<CourierDocument[]> {
    try {
      const { data } = await api.get<CourierDocument[]>(`/couriers/${courierId}/documents`);
      return data;
    } catch (e) {
      console.error('courierDocumentService.getDocuments failed:', e);
      return [];
    }
  },

  async upload(courierId: number, documentTypeId: number, file: File): Promise<DocumentUploadResult> {
    const form = new FormData();
    form.append('file', file);
    form.append('documentTypeId', String(documentTypeId));
    const { data } = await api.post<DocumentUploadResult>(
      `/couriers/${courierId}/documents`,
      form,
      { headers: { 'Content-Type': 'multipart/form-data' } }
    );
    return data;
  },

  async getDownloadUrl(courierId: number, docId: number): Promise<string> {
    const { data } = await api.get<{ url: string }>(`/couriers/${courierId}/documents/${docId}/download`);
    return data.url;
  },

  async delete(courierId: number, docId: number): Promise<void> {
    await api.delete(`/couriers/${courierId}/documents/${docId}`);
  },

  async verify(courierId: number, docId: number): Promise<CourierDocument> {
    const { data } = await api.put<CourierDocument>(`/couriers/${courierId}/documents/${docId}/verify`);
    return data;
  },

  async extractOnly(courierId: number, file: File): Promise<DocumentExtractionResult> {
    const form = new FormData();
    form.append('file', file);
    const { data } = await api.post<DocumentExtractionResult>(
      `/couriers/${courierId}/documents/extract`,
      form,
      { headers: { 'Content-Type': 'multipart/form-data' } }
    );
    return data;
  },
};
