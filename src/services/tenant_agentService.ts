import api from './tenant_api';
import type { Agent, RateCard, Courier, NpContact } from '@/types';

export const agentService = {
  list: (params?: { search?: string; status?: string; association?: string; isNp?: boolean }) =>
    api.get<Agent[]>('/agents', { params }),

  get: (id: number) => api.get<Agent>(`/agents/${id}`),

  create: (data: Partial<Agent>) => api.post<Agent>('/agents', data),

  update: (id: number, data: Partial<Agent>) => api.put<Agent>(`/agents/${id}`, data),

  delete: (id: number) => api.delete(`/agents/${id}`),

  // Rate cards
  getRateCards: (agentId: number) => api.get<RateCard[]>(`/agents/${agentId}/rates`),

  updateRateCard: (agentId: number, rateId: number, data: Partial<RateCard>) =>
    api.put<RateCard>(`/agents/${agentId}/rates/${rateId}`, data),

  // Fleet (NP couriers — read-only for tenant)
  getCouriers: (agentId: number) => api.get<Courier[]>(`/agents/${agentId}/couriers`),

  // Contacts (NP portal users)
  getContacts: (agentId: number) => api.get<NpContact[]>(`/agents/${agentId}/contacts`),

  // NP activation
  activateNp: (agentId: number) => api.post(`/agents/${agentId}/activate-np`),

  deactivateNp: (agentId: number) => api.post(`/agents/${agentId}/deactivate-np`),
};
