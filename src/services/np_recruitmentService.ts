import type { CourierApplicant, ApplicantFilter, PipelineSummary } from '@/types';
import { mockApplicants, mockPipelineSummary } from './np_mockData';

export const recruitmentService = {
  getApplicants: (filters?: ApplicantFilter): CourierApplicant[] => {
    let result = [...mockApplicants];
    if (filters?.stage) result = result.filter(a => a.pipelineStage === filters.stage);
    if (filters?.search) {
      const q = filters.search.toLowerCase();
      result = result.filter(a =>
        `${a.firstName} ${a.lastName} ${a.email}`.toLowerCase().includes(q)
      );
    }
    return result;
  },

  getApplicantById: (id: number): CourierApplicant | undefined =>
    mockApplicants.find(a => a.id === id),

  getPipelineSummary: (): PipelineSummary[] => mockPipelineSummary,

  createApplicant: (data: Partial<CourierApplicant>): CourierApplicant => {
    const newApplicant: CourierApplicant = {
      id: mockApplicants.length + 1,
      tenantId: 1,
      regionId: null,
      email: data.email || '',
      firstName: data.firstName || '',
      lastName: data.lastName || '',
      phone: data.phone || null,
      address: data.address || null,
      city: data.city || null,
      state: data.state || null,
      postcode: data.postcode || null,
      vehicleType: data.vehicleType || null,
      vehicleMake: data.vehicleMake || null,
      vehicleModel: data.vehicleModel || null,
      vehicleYear: data.vehicleYear || null,
      vehiclePlate: data.vehiclePlate || null,
      bankAccountName: data.bankAccountName || null,
      bankAccountNumber: data.bankAccountNumber || null,
      bankBSB: data.bankBSB || null,
      nextOfKinName: data.nextOfKinName || null,
      nextOfKinPhone: data.nextOfKinPhone || null,
      nextOfKinRelationship: data.nextOfKinRelationship || null,
      pipelineStage: 'Registration',
      declarationSigned: false,
      declarationSignedDate: null,
      declarationSignatureS3Key: null,
      rejectedDate: null,
      rejectedReason: null,
      approvedAsCourierId: null,
      createdDate: new Date().toISOString(),
      modifiedDate: null,
      notes: data.notes || null,
    };
    mockApplicants.push(newApplicant);
    return newApplicant;
  },

  advanceStage: (id: number): CourierApplicant | undefined => {
    const stages = ['Registration', 'Email Verification', 'Profile', 'Documentation', 'Declaration/Contract', 'Training', 'Approval'] as const;
    const a = mockApplicants.find(x => x.id === id);
    if (!a) return undefined;
    const idx = stages.indexOf(a.pipelineStage);
    if (idx < stages.length - 1) {
      a.pipelineStage = stages[idx + 1];
      a.modifiedDate = new Date().toISOString();
    }
    return a;
  },

  rejectApplicant: (id: number, reason: string): CourierApplicant | undefined => {
    const a = mockApplicants.find(x => x.id === id);
    if (!a) return undefined;
    a.rejectedDate = new Date().toISOString();
    a.rejectedReason = reason;
    return a;
  },

  approveApplicant: (id: number): CourierApplicant | undefined => {
    const a = mockApplicants.find(x => x.id === id);
    if (!a) return undefined;
    a.pipelineStage = 'Approval';
    a.approvedAsCourierId = 100 + id;
    a.modifiedDate = new Date().toISOString();
    return a;
  },
};
