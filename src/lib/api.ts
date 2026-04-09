const BASE = '/api';

function getToken() {
  return localStorage.getItem('token');
}

async function request<T>(url: string, options?: RequestInit): Promise<T> {
  const token = getToken();
  const headers: Record<string, string> = {
    'Content-Type': 'application/json',
    ...(token ? { Authorization: `Bearer ${token}` } : {}),
  };
  const res = await fetch(`${BASE}${url}`, { ...options, headers: { ...headers, ...(options?.headers as Record<string, string>) } });
  if (!res.ok) throw new Error(`API ${res.status}`);
  return res.json();
}

// Types
export interface PortalConfig {
  tenantId: number;
  companyName: string;
  logoUrl?: string;
  primaryColor?: string;
  secondaryColor?: string;
  steps: PortalStep[];
}

export interface PortalStep {
  id: number;
  stepType: string;
  title: string;
  description?: string;
  sortOrder: number;
  isRequired: boolean;
  isActive: boolean;
  config?: string;
  documentTypeId?: number;
}

export interface Applicant {
  id: number;
  email: string;
  firstName: string;
  lastName: string;
  phone?: string;
  pipelineStage: string;
  createdDate: string;
}

export interface ApplicantDocument {
  id: number;
  applicantId: number;
  documentTypeId: number;
  fileName: string;
  status: string;
  created: string;
}

// API functions
export async function getPortalConfig(tenantSlug: string): Promise<PortalConfig> {
  return request<PortalConfig>(`/portal/config/${tenantSlug}`);
}

export async function getPortalSteps(slug: string) {
  return request<PortalStep[]>(`/portal-steps?slug=${slug}`);
}

export async function applyAsApplicant(data: Record<string, unknown>): Promise<Applicant> {
  return request<Applicant>('/portal/apply', { method: 'POST', body: JSON.stringify(data) });
}

export async function submitApplication(data: Record<string, unknown>) {
  return request<{ id: number }>('/portal/apply', { method: 'POST', body: JSON.stringify(data) });
}

export async function saveStepData(applicantId: number, stepId: number, stepType: string, fieldData: string, aiExtractedData?: string, aiConfirmedFields?: string) {
  return request<void>(`/applicant/${applicantId}/step-data`, {
    method: 'POST',
    body: JSON.stringify({ portalStepId: stepId, stepType, fieldData, aiExtractedData, aiConfirmedFields }),
  });
}

export async function uploadDocument(applicantId: number, file: File, docTypeId: number) {
  const form = new FormData();
  form.append('file', file);
  form.append('docTypeId', String(docTypeId));
  const token = getToken();
  const res = await fetch(`${BASE}/applicant/${applicantId}/upload`, {
    method: 'POST',
    body: form,
    headers: token ? { Authorization: `Bearer ${token}` } : {},
  });
  return res.json();
}

export async function submitQuiz(applicantId: number, quizId: number, answers: unknown[]) {
  return request<{ passed: boolean; score: number }>(`/applicant/${applicantId}/quiz`, {
    method: 'POST',
    body: JSON.stringify({ quizId, answers }),
  });
}

export async function getApplicantByEmail(email: string): Promise<Applicant | null> {
  try {
    return await request<Applicant>(`/applicant/status/${encodeURIComponent(email)}`);
  } catch {
    return null;
  }
}

export async function getApplicantDocuments(applicantId: number): Promise<ApplicantDocument[]> {
  return request<ApplicantDocument[]>(`/applicant/${applicantId}/documents`);
}

export async function getApplicantStatus(email: string) {
  return request<Record<string, unknown>>(`/applicant/status/${encodeURIComponent(email)}`);
}

export async function getTenantBranding(slug: string) {
  return request<{ tenantId: number; companyName: string; logoUrl?: string; primaryColor?: string }>(`/portal/branding/${slug}`);
}
