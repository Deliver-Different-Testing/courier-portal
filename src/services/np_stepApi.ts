import api from './np_api';

// === Types ===

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
  documentType?: DocumentType;
}

export interface DocumentType {
  id: number;
  name: string;
  description?: string;
  isRequired: boolean;
  appliesTo: string;
  validityMonths?: number;
  sortOrder: number;
  isActive: boolean;
}

export interface Quiz {
  id: number;
  title: string;
  description?: string;
  passingScore: number;
  timeLimit?: number;
  questions?: QuizQuestion[];
}

export interface QuizQuestion {
  id: number;
  questionText: string;
  questionType: string;
  options?: string;
  correctAnswer?: string;
}

export interface Applicant {
  id: number;
  firstName: string;
  lastName: string;
  email: string;
  phone?: string;
  status?: string;
}

export interface ApplicantDocument {
  id: number;
  applicantId: number;
  documentTypeId: number;
  fileName: string;
  fileSize: number;
  status: string;
  uploadedDate: string;
  rejectionReason?: string;
  documentType?: DocumentType;
}

// === Portal Steps API ===

export const getPortalSteps = () =>
  api.get<PortalStep[]>('/portal-steps').then(r => r.data);

export const createPortalStep = (step: Partial<PortalStep>) =>
  api.post<PortalStep>('/portal-steps', step).then(r => r.data);

export const updatePortalStep = (id: number, step: Partial<PortalStep>) =>
  api.put<PortalStep>(`/portal-steps/${id}`, step).then(r => r.data);

export const deletePortalStep = (id: number) =>
  api.delete(`/portal-steps/${id}`).then(r => r.data);

export const reorderPortalSteps = (items: { id: number; sortOrder: number }[]) =>
  api.put('/portal-steps/reorder', items).then(r => r.data);

// === Document Scan API ===

export const scanDocument = async (file: File, stepType: string) => {
  const formData = new FormData();
  formData.append('file', file);
  formData.append('stepType', stepType);
  const res = await api.post('/documentscan/scan', formData, {
    headers: { 'Content-Type': 'multipart/form-data' },
  });
  return res.data as { success: boolean; fields?: any; error?: string; fileName?: string; fileSize?: number };
};

// === Settings API ===

export const getSettings = () =>
  api.get('/settings').then(r => r.data);

export const updateSettings = (data: any) =>
  api.put('/settings', data).then(r => r.data);

// === Quiz API ===

export const submitQuiz = (data: {
  applicantId: number;
  quizId: number;
  startedDate?: string;
  timeTaken?: number;
  answers: { questionId: number; answer: string }[];
}) => api.post('/quiz/submit', data).then(r => r.data);

// === Applicant Documents API ===

export const getApplicants = () =>
  api.get<Applicant[]>('/applicants').then(r => r.data);

export const getApplicantDocuments = (applicantId: number) =>
  api.get<ApplicantDocument[]>(`/applicants/${applicantId}/documents`).then(r => r.data);

export const verifyDocument = (docId: number, approved: boolean, reason?: string) =>
  api.put(`/documents/${docId}/verify`, { approved, reason }).then(r => r.data);

export const getDocumentTypes = () =>
  api.get<DocumentType[]>('/document-types').then(r => r.data);

export const createDocumentType = (data: Partial<DocumentType>) =>
  api.post<DocumentType>('/document-types', data).then(r => r.data);

export const updateDocumentType = (id: number, data: Partial<DocumentType>) =>
  api.put<DocumentType>(`/document-types/${id}`, data).then(r => r.data);

// === Portal Config (public) ===

export interface PortalConfig {
  companyName: string;
  welcomeTitle: string;
  welcomeSubtitle: string;
  showVehicleStep: boolean;
  showQuizStep: boolean;
  primaryColor: string;
  steps: PortalStep[];
  documentTypes: DocumentType[];
  quiz: Quiz | null;
}

export const getPortalConfig = () =>
  api.get<PortalConfig>('/settings/portal').then(r => r.data);
