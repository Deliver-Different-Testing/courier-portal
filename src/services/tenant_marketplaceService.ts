import api from './tenant_api';
import type { MarketplacePosting, Quote, CarrierSearchResult } from '@/types';

export const marketplaceService = {
  // Postings
  listPostings: () => api.get<MarketplacePosting[]>('/marketplace/postings'),

  getPosting: (id: number) => api.get<MarketplacePosting>(`/marketplace/postings/${id}`),

  createPosting: (data: Partial<MarketplacePosting>) =>
    api.post<MarketplacePosting>('/marketplace/postings', data),

  // Quotes
  listQuotes: (postingId: number) => api.get<Quote[]>(`/marketplace/postings/${postingId}/quotes`),

  // Discovery
  searchCarriers: (query: string) =>
    api.post<{ results: CarrierSearchResult[]; message: string }>('/marketplace/discover', { query }),

  sendQuoteRequest: (carrierId: number, postingId?: number) =>
    api.post('/marketplace/quote-request', { carrierId, postingId }),
};
