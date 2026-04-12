import api from './np_api';
import type {
  DocumentType,
  CourierDocument,
  DocumentUploadResult,
  DocumentExtractionResult,
  DocumentCategory,
  DocumentAppliesTo,
  DocumentPurpose,
} from '@/types';

// Re-export for consumers
export type { DocumentCategory, DocumentAppliesTo, DocumentPurpose };

/**
 * @backend-needed GET /v1/np/document-types — Loc: build this endpoint
 * @backend-needed POST /v1/np/document-types — Loc: build this endpoint
 * @backend-needed PUT /v1/np/document-types/{id} — Loc: build this endpoint
 * @backend-needed DELETE /v1/np/document-types/{id} — Loc: build this endpoint
 */
export const documentTypeService = {
  /** @backend-needed GET /v1/np/document-types */
  async getAll(): Promise<DocumentType[]> {
    try {
      const { data } = await api.get<DocumentType[]>('/v1/np/document-types');
      return data ?? [];
    } catch (e: any) {
      if (e?.response?.status === 404 || e?.code === 'ERR_NETWORK') {
        // FALLBACK: returns empty array until backend is implemented
        return [];
      }
      console.error('documentTypeService.getAll failed:', e);
      return [];
    }
  },

  /** @backend-needed GET /v1/np/document-types/{id} */
  async getById(id: number): Promise<DocumentType | undefined> {
    try {
      const { data } = await api.get<DocumentType>(`/v1/np/document-types/${id}`);
      return data;
    } catch (e) {
      console.error(`documentTypeService.getById(${id}) failed:`, e);
      return undefined;
    }
  },

  /** @backend-needed POST /v1/np/document-types */
  async create(docType: Omit<DocumentType, 'id'>): Promise<DocumentType> {
    const { data } = await api.post<DocumentType>('/v1/np/document-types', docType);
    return data;
  },

  /** @backend-needed PUT /v1/np/document-types/{id} */
  async update(id: number, updates: Partial<DocumentType>): Promise<DocumentType | undefined> {
    try {
      const { data } = await api.put<DocumentType>(`/v1/np/document-types/${id}`, updates);
      return data;
    } catch (e) {
      console.error(`documentTypeService.update(${id}) failed:`, e);
      return undefined;
    }
  },

  /** @backend-needed DELETE /v1/np/document-types/{id} */
  async deactivate(id: number): Promise<boolean> {
    try {
      await api.delete(`/v1/np/document-types/${id}`);
      return true;
    } catch (e) {
      console.error(`documentTypeService.deactivate(${id}) failed:`, e);
      return false;
    }
  },

  /** @backend-needed PUT /v1/np/document-types/reorder */
  async reorder(items: { id: number; sortOrder: number }[]): Promise<void> {
    await api.put('/v1/np/document-types/reorder', items);
  },

  /** @backend-needed GET /v1/np/document-types/{id}/documents */
  async getDocumentsForType(docTypeId: number): Promise<CourierDocument[]> {
    try {
      const { data } = await api.get<CourierDocument[]>(`/v1/np/document-types/${docTypeId}/documents`);
      return data ?? [];
    } catch (e: any) {
      if (e?.response?.status === 404 || e?.code === 'ERR_NETWORK') {
        return [];
      }
      console.error(`documentTypeService.getDocumentsForType(${docTypeId}) failed:`, e);
      return [];
    }
  },

  /** @backend-needed POST /v1/np/document-types/{id}/extract */
  async extractDocument(docTypeId: number, file: File): Promise<DocumentExtractionResult | null> {
    try {
      const form = new FormData();
      form.append('file', file);
      const { data } = await api.post<DocumentExtractionResult>(`/v1/np/document-types/${docTypeId}/extract`, form, {
        headers: { 'Content-Type': 'multipart/form-data' },
      });
      return data;
    } catch (e) {
      console.error(`documentTypeService.extractDocument(${docTypeId}) failed:`, e);
      return null;
    }
  },

  /** @backend-needed POST /v1/np/document-types/seed */
  async seed(): Promise<DocumentType[]> {
    try {
      const { data } = await api.post<DocumentType[]>('/v1/np/document-types/seed', {});
      return data ?? [];
    } catch (e) {
      console.error('documentTypeService.seed failed:', e);
      return [];
    }
  },

  /** @backend-needed POST /v1/np/document-types/{id}/template */
  async uploadTemplate(id: number, file: File): Promise<boolean> {
    try {
      const form = new FormData();
      form.append('file', file);
      await api.post(`/v1/np/document-types/${id}/template`, form, {
        headers: { 'Content-Type': 'multipart/form-data' },
      });
      return true;
    } catch (e) {
      console.error(`documentTypeService.uploadTemplate(${id}) failed:`, e);
      return false;
    }
  },
};

/**
 * @backend-needed GET /v1/np/couriers/{courierId}/documents — Loc: build this endpoint
 * @backend-needed POST /v1/np/couriers/{courierId}/documents — Loc: build this endpoint
 * @backend-needed DELETE /v1/np/couriers/{courierId}/documents/{docId} — Loc: build this endpoint
 * @backend-needed POST /v1/np/couriers/{courierId}/documents/{docId}/verify — Loc: build this endpoint
 */
export const courierDocumentService = {
  /** @backend-needed GET /v1/np/couriers/{courierId}/documents */
  async getDocuments(courierId: number): Promise<CourierDocument[]> {
    try {
      const { data } = await api.get<CourierDocument[]>(`/v1/np/couriers/${courierId}/documents`);
      return data ?? [];
    } catch (e: any) {
      if (e?.response?.status === 404 || e?.code === 'ERR_NETWORK') {
        // FALLBACK: returns empty array until backend is implemented
        return [];
      }
      console.error(`courierDocumentService.getDocuments(${courierId}) failed:`, e);
      return [];
    }
  },

  /** @backend-needed POST /v1/np/couriers/{courierId}/documents */
  async upload(courierId: number, documentTypeId: number, file: File): Promise<DocumentUploadResult> {
    const form = new FormData();
    form.append('file', file);
    form.append('documentTypeId', String(documentTypeId));
    const { data } = await api.post<DocumentUploadResult>(`/v1/np/couriers/${courierId}/documents`, form, {
      headers: { 'Content-Type': 'multipart/form-data' },
    });
    return data;
  },

  /** @backend-needed DELETE /v1/np/couriers/{courierId}/documents/{docId} */
  async delete(courierId: number, docId: number): Promise<void> {
    await api.delete(`/v1/np/couriers/${courierId}/documents/${docId}`);
  },

  /** @backend-needed POST /v1/np/couriers/{courierId}/documents/{docId}/verify */
  async verify(courierId: number, docId: number): Promise<CourierDocument> {
    const { data } = await api.post<CourierDocument>(`/v1/np/couriers/${courierId}/documents/${docId}/verify`, {});
    return data;
  },

  /** @backend-needed GET /v1/np/couriers/{courierId}/documents/{docId}/download-url */
  async getDownloadUrl(courierId: number, docId: number): Promise<string | null> {
    try {
      const { data } = await api.get<{ url: string }>(`/v1/np/couriers/${courierId}/documents/${docId}/download-url`);
      return data.url;
    } catch (e) {
      console.error(`courierDocumentService.getDownloadUrl(${courierId}, ${docId}) failed:`, e);
      return null;
    }
  },

  /** @backend-needed POST /v1/np/couriers/{courierId}/documents/extract */
  async extractOnly(courierId: number, file: File): Promise<DocumentExtractionResult | null> {
    try {
      const form = new FormData();
      form.append('file', file);
      const { data } = await api.post<DocumentExtractionResult>(`/v1/np/couriers/${courierId}/documents/extract`, form, {
        headers: { 'Content-Type': 'multipart/form-data' },
      });
      return data;
    } catch (e) {
      console.error(`courierDocumentService.extractOnly(${courierId}) failed:`, e);
      return null;
    }
  },
};
