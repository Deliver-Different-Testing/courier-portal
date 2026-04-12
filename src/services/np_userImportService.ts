import api from './np_api';
import type { UploadResult, AiColumnSuggestion, AiMapResponse } from './np_importService';

export interface UserColumnMapping {
  firstName?: string;
  lastName?: string;
  email?: string;
  phone?: string;
  role?: string;
  jobTitle?: string;
  department?: string;
  notes?: string;
}

export interface ValidatedUserRow {
  rowNumber: number;
  data: Record<string, string>;
  status: 'valid' | 'duplicate' | 'error';
  errors: string[];
}

export interface UserValidationResult {
  rows: ValidatedUserRow[];
  validCount: number;
  duplicateCount: number;
  errorCount: number;
}

export interface UserImportResult {
  totalRows: number;
  successCount: number;
  failedCount: number;
  failedRows: { rowNumber: number; data: Record<string, string>; error: string }[];
}

export const userImportService = {
  async uploadFile(file: File): Promise<UploadResult> {
    const form = new FormData();
    form.append('file', file);
    const { data } = await api.post<UploadResult>('/v1/np/user-import/upload', form, {
      headers: { 'Content-Type': 'multipart/form-data' },
    });
    return data;
  },

  async aiMapColumns(headers: string[], sampleRows: Record<string, string>[]): Promise<AiMapResponse> {
    const { data } = await api.post<AiMapResponse>('/v1/np/user-import/ai-map', {
      headers,
      sampleRows: sampleRows.slice(0, 3),
    });
    return data;
  },

  async validate(rows: Record<string, string>[], mapping: UserColumnMapping): Promise<UserValidationResult> {
    const { data } = await api.post<UserValidationResult>('/v1/np/user-import/validate', { rows, mapping });
    return data;
  },

  async execute(rows: ValidatedUserRow[]): Promise<UserImportResult> {
    const { data } = await api.post<UserImportResult>('/v1/np/user-import/execute', { rows });
    return data;
  },

  parsePastedData(text: string): UploadResult {
    const lines = text.split('\n').filter((l) => l.trim());
    if (lines.length < 2) throw new Error('Data must have a header row and at least one data row.');
    const delimiter = lines[0].includes('\t') ? '\t' : ',';
    const columns = lines[0].split(delimiter).map((c) => c.trim());
    const allRows: Record<string, string>[] = [];
    for (let i = 1; i < lines.length; i++) {
      const fields = lines[i].split(delimiter);
      const row: Record<string, string> = {};
      let hasData = false;
      columns.forEach((col, ci) => {
        const val = (fields[ci] || '').trim();
        row[col] = val;
        if (val) hasData = true;
      });
      if (hasData) allRows.push(row);
    }
    return { columns, previewRows: allRows.slice(0, 5), totalRows: allRows.length, allRows };
  },
};

// Re-export shared types
export type { UploadResult, AiColumnSuggestion, AiMapResponse };
