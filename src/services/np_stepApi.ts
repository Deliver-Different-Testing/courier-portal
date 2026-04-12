// Step API service — wired to real backend endpoints
const BASE = '/api';

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

export interface Quiz {
  id: number;
  title: string;
  description?: string;
  passMark: number;
  questions: Array<{
    id: number;
    questionText: string;
    questionType: string;
    options: Array<{ id: number; optionText: string; isCorrect: boolean }>;
  }>;
}

function authHeaders(): Record<string, string> {
  const token = localStorage.getItem('portal_token');
  return token ? { Authorization: `Bearer ${token}` } : {};
}

export async function getSteps(): Promise<PortalStep[]> {
  const res = await fetch(`${BASE}/portal-steps`, { headers: authHeaders() });
  return res.ok ? res.json() : [];
}

export async function saveStep(step: Partial<PortalStep>) {
  const method = step.id ? 'PUT' : 'POST';
  const url = step.id ? `${BASE}/portal-steps/${step.id}` : `${BASE}/portal-steps`;
  const res = await fetch(url, {
    method,
    headers: { 'Content-Type': 'application/json', ...authHeaders() },
    body: JSON.stringify(step),
  });
  return res.json();
}

export async function deleteStep(id: number) {
  await fetch(`${BASE}/portal-steps/${id}`, { method: 'DELETE', headers: authHeaders() });
}

export async function reorderSteps(items: Array<{ id: number; sortOrder: number }>) {
  await fetch(`${BASE}/portal-steps/reorder`, {
    method: 'PUT',
    headers: { 'Content-Type': 'application/json', ...authHeaders() },
    body: JSON.stringify(items),
  });
}

/**
 * Save applicant step data.
 * @backend POST /api/Applicant/{id}/step-data
 */
export async function saveStepData(applicantId: number, stepId: number, data: Record<string, unknown>) {
  const res = await fetch(`${BASE}/Applicant/${applicantId}/step-data`, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json', ...authHeaders() },
    body: JSON.stringify({ portalStepId: stepId, stepType: 'generic', fieldData: JSON.stringify(data) }),
  });
  return res.json();
}

/**
 * Upload a document for an applicant.
 * @backend POST /api/Applicant/{id}/upload (path derived from ApplicantController patterns)
 */
export async function uploadDocument(applicantId: number, file: File, docTypeId: number) {
  const form = new FormData();
  form.append('file', file);
  form.append('docTypeId', String(docTypeId));
  const res = await fetch(`${BASE}/Applicant/${applicantId}/upload`, {
    method: 'POST',
    headers: authHeaders(),
    body: form,
  });
  return res.json();
}

/**
 * AI document scan via DocumentScanController.
 * @backend POST /api/DocumentScan/scan
 * Accepts: multipart/form-data with 'file' (IFormFile) and 'stepType' (string)
 */
export async function scanDocument(
  file: File,
  stepType = 'generic'
): Promise<{ success: boolean; fields: Record<string, unknown>; confidence?: number; error?: string }> {
  const form = new FormData();
  form.append('file', file);
  form.append('stepType', stepType);
  try {
    const res = await fetch(`${BASE}/DocumentScan/scan`, {
      method: 'POST',
      headers: authHeaders(),
      body: form,
    });
    if (!res.ok) {
      return { success: false, fields: {}, error: `HTTP ${res.status}` };
    }
    const result = await res.json();
    return {
      success: result.success ?? false,
      fields: result.fields ?? {},
      confidence: result.confidence,
      error: result.error,
    };
  } catch (e) {
    console.error('scanDocument failed:', e);
    return { success: false, fields: {}, error: 'Scan request failed' };
  }
}

/**
 * Submit a quiz attempt for an applicant.
 * @backend POST /api/quizzes/attempts
 * Body: { quizDefinitionId, courierId, answers: [{ questionId, selectedOptionIds }] }
 */
export async function submitQuiz(
  applicantId: number,
  quizId: number,
  answers: { questionId: number; selectedOptionIds: number[] }[]
) {
  const res = await fetch(`${BASE}/quizzes/attempts`, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json', ...authHeaders() },
    body: JSON.stringify({
      quizDefinitionId: quizId,
      courierId: applicantId,
      answers,
    }),
  });
  return res.json();
}
