// Step API service
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

export async function getSteps(): Promise<PortalStep[]> {
  const res = await fetch(`${BASE}/portal-steps`);
  return res.ok ? res.json() : [];
}

export async function saveStep(step: Partial<PortalStep>) {
  const method = step.id ? 'PUT' : 'POST';
  const url = step.id ? `${BASE}/portal-steps/${step.id}` : `${BASE}/portal-steps`;
  const res = await fetch(url, { method, headers: { 'Content-Type': 'application/json' }, body: JSON.stringify(step) });
  return res.json();
}

export async function deleteStep(id: number) {
  await fetch(`${BASE}/portal-steps/${id}`, { method: 'DELETE' });
}

export async function reorderSteps(items: Array<{ id: number; sortOrder: number }>) {
  await fetch(`${BASE}/portal-steps/reorder`, { method: 'PUT', headers: { 'Content-Type': 'application/json' }, body: JSON.stringify(items) });
}

export async function saveStepData(applicantId: number, stepId: number, data: Record<string, unknown>) {
  const res = await fetch(`${BASE}/applicant/${applicantId}/step-data`, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({ portalStepId: stepId, stepType: 'generic', fieldData: JSON.stringify(data) }),
  });
  return res.json();
}

export async function uploadDocument(applicantId: number, file: File, docTypeId: number) {
  const form = new FormData();
  form.append('file', file);
  form.append('docTypeId', String(docTypeId));
  const res = await fetch(`${BASE}/applicant/${applicantId}/upload`, { method: 'POST', body: form });
  return res.json();
}

export async function scanDocument(_file: File): Promise<{ success: boolean; fields: Record<string, string>; confidence: number }> {
  // TODO: AI document scanning
  return { success: false, fields: {}, confidence: 0 };
}

export async function submitQuiz(applicantId: number, quizId: number, answers: unknown[]) {
  const res = await fetch(`${BASE}/applicant/${applicantId}/quiz`, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({ quizId, answers }),
  });
  return res.json();
}
