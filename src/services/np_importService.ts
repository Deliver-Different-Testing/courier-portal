import api from './np_api';

export interface UploadResult {
  columns: string[];
  previewRows: Record<string, string>[];
  totalRows: number;
  allRows?: Record<string, string>[];
}

export interface CourierColumnMapping {
  firstName?: string;
  lastName?: string;
  email?: string;
  phone?: string;
  vehicleType?: string;
  licenseRego?: string;
  zones?: string;
  address?: string;
  emergencyContactName?: string;
  emergencyContactPhone?: string;
  notes?: string;
}

export interface AiColumnSuggestion {
  systemField: string;
  mappedColumn: string | null;
  confidence: 'high' | 'medium' | 'low';
  confidenceScore: number;
  reasoning: string | null;
}

export interface AiMapResponse {
  suggestions: AiColumnSuggestion[];
  unmappedHeaders: string[];
}

export interface ValidatedCourierRow {
  rowNumber: number;
  data: Record<string, string>;
  status: 'valid' | 'duplicate' | 'error';
  errors: string[];
}

export interface CourierValidationResult {
  rows: ValidatedCourierRow[];
  validCount: number;
  duplicateCount: number;
  errorCount: number;
}

export interface CourierImportResult {
  totalRows: number;
  successCount: number;
  failedCount: number;
  failedRows: { rowNumber: number; data: Record<string, string>; error: string }[];
}

export const courierImportService = {
  async uploadFile(file: File): Promise<UploadResult> {
    const form = new FormData();
    form.append('file', file);
    const { data } = await api.post<UploadResult>('/v1/np/courier-import/upload', form, {
      headers: { 'Content-Type': 'multipart/form-data' },
    });
    return data;
  },

  async aiMapColumns(headers: string[], sampleRows: Record<string, string>[]): Promise<AiMapResponse> {
    const { data } = await api.post<AiMapResponse>('/v1/np/courier-import/ai-map', {
      headers,
      sampleRows: sampleRows.slice(0, 3),
    });
    return data;
  },

  async validate(rows: Record<string, string>[], mapping: CourierColumnMapping): Promise<CourierValidationResult> {
    const { data } = await api.post<CourierValidationResult>('/v1/np/courier-import/validate', { rows, mapping });
    return data;
  },

  async execute(rows: ValidatedCourierRow[]): Promise<CourierImportResult> {
    const { data } = await api.post<CourierImportResult>('/v1/np/courier-import/execute', { rows });
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
