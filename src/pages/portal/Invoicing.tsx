import { useState, useEffect, useCallback } from 'react';
import {
  getRecentInvoices, getPastInvoices, getInvoice, getUninvoiced, createInvoice,
  type Invoice, type PastInvoice, type UninvoicedData, type UninvoicedMaster,
} from '@/services/portal_invoiceService';

type Section = 'list' | 'create' | 'view';

function fmt(n: number) { return `$${n.toFixed(2)}`; }
function fmtDate(iso: string) {
  return new Date(iso).toLocaleDateString('en-NZ', { day: 'numeric', month: 'short', year: 'numeric' });
}
function fmtPct(n: number) {
  // API may return 0.15 or 15 — normalise to display "15%"
  return n < 1 ? `${(n * 100).toFixed(0)}` : `${n.toFixed(0)}`;
}

export default function PortalInvoicing() {
  const [section, setSection] = useState<Section>('list');
  const [viewInvoice, setViewInvoice] = useState<Invoice | null>(null);
  const [listMode, setListMode] = useState<'recent' | 'past'>('recent');
  const [pastLimit, setPastLimit] = useState(10);

  // Data
  const [recentInvoices, setRecentInvoices] = useState<Invoice[]>([]);
  const [pastInvoices, setPastInvoices] = useState<PastInvoice[]>([]);
  const [uninvoiced, setUninvoiced] = useState<UninvoicedData | null>(null);

  // Loading / error
  const [loading, setLoading] = useState(true);
  const [creating, setCreating] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [successMsg, setSuccessMsg] = useState<string | null>(null);

  const loadInitial = useCallback(async () => {
    setLoading(true);
    setError(null);
    try {
      const [inv, unin] = await Promise.all([getRecentInvoices(), getUninvoiced()]);
      setRecentInvoices(inv);
      setUninvoiced(unin);
    } catch (e: any) {
      setError(e?.response?.data?.message || e.message || 'Failed to load invoicing data');
    } finally {
      setLoading(false);
    }
  }, []);

  useEffect(() => { loadInitial(); }, [loadInitial]);

  const loadPast = useCallback(async () => {
    setLoading(true);
    setError(null);
    try {
      const inv = await getPastInvoices();
      setPastInvoices(inv);
    } catch (e: any) {
      setError(e?.response?.data?.message || e.message || 'Failed to load past invoices');
    } finally {
      setLoading(false);
    }
  }, []);

  const handleListModeChange = (mode: 'recent' | 'past') => {
    setListMode(mode);
    if (mode === 'past' && pastInvoices.length === 0) loadPast();
  };

  const openInvoice = async (invoiceNo: string) => {
    setLoading(true);
    setError(null);
    try {
      const inv = await getInvoice(invoiceNo);
      setViewInvoice(inv);
      setSection('view');
    } catch (e: any) {
      setError(e?.response?.data?.message || e.message || 'Failed to load invoice');
    } finally {
      setLoading(false);
    }
  };

  const handleCreate = async () => {
    setCreating(true);
    setError(null);
    try {
      const inv = await createInvoice();
      setSuccessMsg(`Invoice ${inv.invoiceNo} created successfully!`);
      setSection('list');
      // Refresh data
      const [recent, unin] = await Promise.all([getRecentInvoices(), getUninvoiced()]);
      setRecentInvoices(recent);
      setUninvoiced(unin);
      setTimeout(() => setSuccessMsg(null), 5000);
    } catch (e: any) {
      setError(e?.response?.data?.message || e.message || 'Failed to create invoice');
    } finally {
      setCreating(false);
    }
  };

  // Validation for create
  const canCreate = uninvoiced
    && (uninvoiced.courier.total > 0 || uninvoiced.masters.length > 0)
    && uninvoiced.masters.every(m => m.total > 0);

  if (loading && !uninvoiced && recentInvoices.length === 0) {
    return <div className="flex items-center justify-center py-20 text-text-muted text-sm">Loading...</div>;
  }

  if (section === 'view' && viewInvoice) {
    return <InvoiceView invoice={viewInvoice} onBack={() => setSection('list')} />;
  }

  if (section === 'create' && uninvoiced) {
    return <InvoiceCreate uninvoiced={uninvoiced} creating={creating} canCreate={!!canCreate} onCreate={handleCreate} onBack={() => setSection('list')} error={error} />;
  }

  return (
    <div className="space-y-4">
      {error && <div className="bg-error/10 text-error text-sm p-3 rounded-xl">{error}</div>}
      {successMsg && <div className="bg-success/10 text-success text-sm p-3 rounded-xl">{successMsg}</div>}

      {/* Uninvoiced Total */}
      <div className="bg-brand-dark rounded-xl p-5 text-center">
        <div className="text-3xl font-bold text-white">{uninvoiced ? fmt(uninvoiced.courier.total) : '--'}</div>
        <div className="text-white/60 text-xs mt-1">To Invoice</div>
        {uninvoiced && uninvoiced.masters.length > 0 && (
          <div className="text-white/40 text-xs mt-1">
            + {uninvoiced.masters.length} master section{uninvoiced.masters.length > 1 ? 's' : ''}
          </div>
        )}
      </div>

      {/* Create Button */}
      <button
        onClick={() => setSection('create')}
        disabled={!canCreate}
        className="w-full bg-brand-cyan text-white font-semibold py-3 rounded-xl flex items-center justify-center gap-2 text-sm hover:bg-brand-cyan/90 transition-colors disabled:opacity-50"
      >
        ➕ Create Invoice
      </button>
      {uninvoiced && !canCreate && (
        <div className="text-center text-xs text-text-muted">No runs to invoice</div>
      )}

      {/* Toggle Recent/Past */}
      <div className="flex gap-2">
        <button
          onClick={() => handleListModeChange('recent')}
          className={`flex-1 py-2 rounded-lg text-xs font-medium ${listMode === 'recent' ? 'bg-brand-cyan text-white' : 'bg-white border border-border text-text-muted'}`}
        >
          Recent Invoices
        </button>
        <button
          onClick={() => handleListModeChange('past')}
          className={`flex-1 py-2 rounded-lg text-xs font-medium ${listMode === 'past' ? 'bg-brand-cyan text-white' : 'bg-white border border-border text-text-muted'}`}
        >
          Past Invoices
        </button>
      </div>

      {/* Invoice List */}
      {loading && <div className="text-center text-sm text-text-muted py-4">Loading...</div>}
      <div className="space-y-2">
        {listMode === 'recent' && recentInvoices.map(inv => (
          <button
            key={inv.invoiceNo}
            onClick={() => openInvoice(inv.invoiceNo)}
            className="w-full bg-white rounded-xl border border-border p-3 flex items-center justify-between text-left hover:border-brand-cyan transition-colors"
          >
            <div>
              <div className="text-sm font-medium">{inv.invoiceNo}</div>
              <div className="text-xs text-text-muted">{fmtDate(inv.created)}</div>
            </div>
            <div className="text-right">
              <div className="text-sm font-bold">{fmt(inv.total)}</div>
            </div>
          </button>
        ))}
        {listMode === 'past' && pastInvoices.slice(0, pastLimit).map(inv => (
          <button
            key={inv.invoiceNo}
            onClick={() => openInvoice(inv.invoiceNo)}
            className="w-full bg-white rounded-xl border border-border p-3 flex items-center justify-between text-left hover:border-brand-cyan transition-colors"
          >
            <div>
              <div className="text-sm font-medium">{inv.invoiceNo}</div>
              <div className="text-xs text-text-muted">{fmtDate(inv.created)}</div>
            </div>
            <div className="text-right text-xs text-brand-cyan">View →</div>
          </button>
        ))}
      </div>

      {listMode === 'recent' && recentInvoices.length === 0 && !loading && (
        <div className="text-center text-sm text-text-muted py-4">No recent invoices</div>
      )}
      {listMode === 'past' && pastInvoices.length === 0 && !loading && (
        <div className="text-center text-sm text-text-muted py-4">No past invoices</div>
      )}

      {listMode === 'past' && pastLimit < pastInvoices.length && (
        <button onClick={() => setPastLimit(p => p + 10)} className="w-full text-center text-sm text-brand-cyan py-2">
          Show more...
        </button>
      )}
    </div>
  );
}

/* ── Uninvoiced Section Renderer ─────────────────────── */

function UninvoicedSectionView({ section, label }: { section: UninvoicedData['courier'] | UninvoicedMaster; label?: string }) {
  return (
    <div className="border-t border-border pt-3 mt-3">
      {label && <div className="text-xs font-bold text-text-muted mb-2">{label}</div>}

      <div className="space-y-2">
        {section.runs.map((run, i) => (
          <div key={i} className="flex justify-between text-xs">
            <span>{fmtDate(run.bookDate)} : {run.runName}</span>
            <span className="font-medium">{fmt(run.amount)}</span>
          </div>
        ))}
      </div>

      {section.runs.length === 0 && (
        <div className="text-xs text-text-muted italic">No runs</div>
      )}

      <div className="border-t border-border mt-3 pt-3 space-y-1 text-xs">
        {(section.gstAmount !== 0 || section.withholdingTaxAmount !== 0) && (
          <div className="flex justify-between">
            <span className="text-text-muted">Subtotal</span>
            <span>{fmt(section.subtotal)}</span>
          </div>
        )}
        {section.gstAmount !== 0 && (
          <div className="flex justify-between">
            <span className="text-text-muted">GST ({fmtPct(section.gstPercentage)}%)</span>
            <span>{fmt(section.gstAmount)}</span>
          </div>
        )}
        {section.withholdingTaxAmount !== 0 && (
          <div className="flex justify-between">
            <span className="text-text-muted">Withholding Tax ({fmtPct(section.withholdingTaxPercentage)}%)</span>
            <span>-{fmt(Math.abs(section.withholdingTaxAmount))}</span>
          </div>
        )}
        <div className="flex justify-between font-bold text-sm pt-1 border-t border-border">
          <span>Total</span>
          <span>{fmt(section.total)}</span>
        </div>
      </div>
    </div>
  );
}

/* ── Create Invoice View ─────────────────────────────── */

function InvoiceCreate({ uninvoiced, creating, canCreate, onCreate, onBack, error }: {
  uninvoiced: UninvoicedData; creating: boolean; canCreate: boolean; onCreate: () => void; onBack: () => void; error: string | null;
}) {
  return (
    <div className="space-y-4">
      <button onClick={onBack} className="text-sm text-brand-cyan">← Back</button>

      {error && <div className="bg-error/10 text-error text-sm p-3 rounded-xl">{error}</div>}

      <div className="bg-white rounded-xl border border-border p-4">
        <div className="text-center text-xs font-bold text-text-muted mb-3">TAX INVOICE PREVIEW</div>

        {/* To Address */}
        <div className="text-xs mb-4">
          <div className="text-text-muted">TO</div>
          <div className="font-medium whitespace-pre-line">{uninvoiced.toAddress}</div>
        </div>

        {/* Courier Section */}
        <div className="text-xs font-bold text-text-muted mb-1">YOUR RUNS</div>
        <UninvoicedSectionView section={uninvoiced.courier} />

        {/* Master Sections */}
        {uninvoiced.masters.map((master, i) => (
          <UninvoicedSectionView
            key={master.masterId}
            section={master}
            label={`MASTER COURIER #${master.masterId}`}
          />
        ))}
      </div>

      <button
        onClick={onCreate}
        disabled={creating || !canCreate}
        className="w-full bg-brand-cyan text-white font-semibold py-3 rounded-xl text-sm disabled:opacity-50"
      >
        {creating ? '⏳ Creating...' : 'Create Invoice'}
      </button>
    </div>
  );
}

/* ── View Invoice ────────────────────────────────────── */

function InvoiceView({ invoice, onBack }: { invoice: Invoice; onBack: () => void }) {
  return (
    <div className="space-y-4">
      <button onClick={onBack} className="text-sm text-brand-cyan">← Back</button>

      <div className="bg-white rounded-xl border border-border p-4">
        <div className="mb-3">
          <div className="text-xs font-bold text-text-muted">TAX INVOICE</div>
          <div className="text-lg font-bold">{invoice.invoiceNo}</div>
        </div>

        <div className="grid grid-cols-2 gap-2 text-xs mb-4">
          <div><span className="text-text-muted">Date:</span> {fmtDate(invoice.created)}</div>
          <div><span className="text-text-muted">Ref:</span> {invoice.reference}</div>
          {invoice.taxNo && <div><span className="text-text-muted">Tax No:</span> {invoice.taxNo}</div>}
        </div>

        {invoice.toAddress && (
          <div className="text-xs mb-4">
            <div className="text-text-muted">TO</div>
            <div className="font-medium whitespace-pre-line">{invoice.toAddress}</div>
          </div>
        )}

        {/* Lines */}
        <div className="border-t border-border pt-3 space-y-2">
          {invoice.lines.map((line, i) => (
            <div key={i} className="flex justify-between text-xs">
              <span>{line.description}{line.quantity > 1 ? ` ×${line.quantity}` : ''}</span>
              <span className="font-medium">{fmt(line.total)}</span>
            </div>
          ))}
        </div>

        {/* Deductions */}
        {invoice.deductions && invoice.deductions.length > 0 && (
          <div className="border-t border-border mt-3 pt-3 space-y-2">
            <div className="text-xs font-bold text-text-muted">DEDUCTIONS</div>
            {invoice.deductions.map((ded, i) => (
              <div key={i} className="flex justify-between text-xs">
                <span>{ded.description}</span>
                <span className="font-medium text-error">{fmt(ded.total)}</span>
              </div>
            ))}
          </div>
        )}

        {/* Totals */}
        <div className="border-t border-border mt-3 pt-3 space-y-1 text-xs">
          {(invoice.gstAmount !== 0 || invoice.withholdingTaxAmount !== 0) && (
            <div className="flex justify-between">
              <span className="text-text-muted">Subtotal</span>
              <span>{fmt(invoice.subtotal)}</span>
            </div>
          )}
          {invoice.gstAmount !== 0 && (
            <div className="flex justify-between">
              <span className="text-text-muted">GST ({fmtPct(invoice.gstPercentage)}%)</span>
              <span>{fmt(invoice.gstAmount)}</span>
            </div>
          )}
          {invoice.withholdingTaxAmount !== 0 && (
            <div className="flex justify-between">
              <span className="text-text-muted">Withholding Tax ({fmtPct(invoice.withholdingTaxPercentage)}%)</span>
              <span>-{fmt(Math.abs(invoice.withholdingTaxAmount))}</span>
            </div>
          )}
          <div className="flex justify-between font-bold text-sm pt-1 border-t border-border">
            <span>Total</span>
            <span>{fmt(invoice.total)}</span>
          </div>
        </div>
      </div>
    </div>
  );
}
