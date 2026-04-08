import portalApi from './portal_api';

// ── Types ──────────────────────────────────────────────

export interface InvoiceLine {
  description: string;
  quantity: number;
  unitPrice: number;
  total: number;
}

export interface Invoice {
  invoiceNo: string;
  created: string;
  reference: string;
  taxNo: string;
  toAddress?: string;
  gstPercentage: number;
  withholdingTaxPercentage: number;
  subtotal: number;
  gstAmount: number;
  withholdingTaxAmount: number;
  total: number;
  lines: InvoiceLine[];
  deductions: InvoiceLine[];
}

export interface PastInvoice {
  invoiceNo: string;
  created: string;
}

export interface UninvoicedRun {
  runType: number;
  bookDate: string;
  runName: string;
  amount: number;
  jobs?: any[];
}

export interface UninvoicedSection {
  runs: UninvoicedRun[];
  subtotal: number;
  gstPercentage: number;
  gstAmount: number;
  withholdingTaxPercentage: number;
  withholdingTaxAmount: number;
  total: number;
}

export interface UninvoicedMaster extends UninvoicedSection {
  masterId: number;
}

export interface UninvoicedData {
  toAddress: string;
  courier: UninvoicedSection;
  masters: UninvoicedMaster[];
}

// ── API calls ──────────────────────────────────────────

export async function getRecentInvoices(): Promise<Invoice[]> {
  const { data } = await portalApi.get<{ success: boolean; invoices: Invoice[] }>('/Invoices/Recent');
  return data.invoices;
}

export async function getPastInvoices(): Promise<PastInvoice[]> {
  const { data } = await portalApi.get<{ success: boolean; invoices: PastInvoice[] }>('/Invoices/Past');
  return data.invoices;
}

export async function getInvoice(invoiceNo: string): Promise<Invoice> {
  const { data } = await portalApi.get<{ success: boolean; invoice: Invoice }>(`/Invoices/${encodeURIComponent(invoiceNo)}`);
  return data.invoice;
}

export async function getUninvoiced(): Promise<UninvoicedData> {
  const { data } = await portalApi.get<{ success: boolean } & UninvoicedData>('/Invoices/Uninvoiced');
  return { toAddress: data.toAddress, courier: data.courier, masters: data.masters };
}

export async function createInvoice(): Promise<Invoice> {
  const { data } = await portalApi.post<{ success: boolean; invoice: Invoice }>('/Invoices');
  return data.invoice;
}
