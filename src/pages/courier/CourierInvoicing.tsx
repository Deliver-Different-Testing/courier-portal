import { useState } from 'react';
import { useParams } from 'react-router-dom';
import { resolveTenant } from '@/lib/tenants';

interface InvoiceLine {
  id: number;
  date: string;
  runName: string;
  description: string;
  amount: number;
  selected: boolean;
}

interface Invoice {
  id: number;
  invoiceNumber: string;
  date: string;
  subtotal: number;
  tax: number;
  total: number;
  status: 'Draft' | 'Submitted' | 'Paid';
}

const MOCK_LINES: InvoiceLine[] = [
  { id: 1, date: '2026-03-05', runName: 'AKL-AM', description: 'Auckland CBD run – 8 stops', amount: 310.25, selected: false },
  { id: 2, date: '2026-03-04', runName: 'AKL-PM', description: 'South Auckland – 5 stops', amount: 195.00, selected: false },
  { id: 3, date: '2026-03-03', runName: 'AKL-AM', description: 'Auckland CBD run – 12 stops', amount: 420.50, selected: false },
];

const MOCK_INVOICES: Invoice[] = [
  { id: 1, invoiceNumber: 'INV-2026-0042', date: '2026-02-28', subtotal: 2450.00, tax: 367.50, total: 2817.50, status: 'Paid' },
  { id: 2, invoiceNumber: 'INV-2026-0051', date: '2026-03-07', subtotal: 1890.00, tax: 283.50, total: 2173.50, status: 'Submitted' },
];

export default function CourierInvoicing() {
  const { tenantSlug } = useParams<{ tenantSlug: string }>();
  const tenant = resolveTenant(tenantSlug);

  // AllowInvoicing flag — in real app, comes from fleet config API
  const allowInvoicing = true;
  const isNZ = tenant.country === 'NZ';
  const taxRate = isNZ ? 0.15 : 0.28;
  const taxLabel = isNZ ? 'GST (15%)' : 'Withholding Tax (28%)';

  const [lines, setLines] = useState(MOCK_LINES);
  const [tab, setTab] = useState<'create' | 'history'>('create');

  const selectedLines = lines.filter(l => l.selected);
  const subtotal = selectedLines.reduce((sum, l) => sum + l.amount, 0);
  const tax = subtotal * taxRate;
  const total = subtotal + tax;

  const toggleLine = (id: number) => {
    setLines(prev => prev.map(l => l.id === id ? { ...l, selected: !l.selected } : l));
  };

  const handleSubmit = () => {
    if (selectedLines.length === 0) return;
    alert(`Invoice submitted: $${total.toFixed(2)} (${selectedLines.length} line items)`);
    setLines(prev => prev.filter(l => !l.selected));
  };

  if (!allowInvoicing) {
    // Read-only: view buyer-created invoices
    return (
      <div className="px-4 pt-4">
        <h1 className="text-xl font-bold text-gray-900 mb-4">Tax Invoices</h1>
        <p className="text-sm text-gray-500 mb-4">Invoices created by {tenant.name}.</p>
        {MOCK_INVOICES.map(inv => (
          <div key={inv.id} className="bg-white rounded-xl p-4 shadow-sm border border-gray-100 mb-3">
            <div className="flex justify-between items-start">
              <div>
                <div className="font-semibold text-sm text-gray-900">{inv.invoiceNumber}</div>
                <div className="text-xs text-gray-400">{inv.date}</div>
              </div>
              <div className="text-right">
                <div className="font-bold text-sm" style={{ color: tenant.accentColor }}>${inv.total.toFixed(2)}</div>
                <span className={`text-xs font-medium px-2 py-0.5 rounded-full ${
                  inv.status === 'Paid' ? 'bg-green-50 text-green-700' : 'bg-amber-50 text-amber-700'
                }`}>{inv.status}</span>
              </div>
            </div>
            <div className="mt-2 text-xs text-gray-500 flex justify-between">
              <span>Subtotal: ${inv.subtotal.toFixed(2)}</span>
              <span>{taxLabel}: ${inv.tax.toFixed(2)}</span>
            </div>
          </div>
        ))}
      </div>
    );
  }

  return (
    <div className="px-4 pt-4">
      <h1 className="text-xl font-bold text-gray-900 mb-4">Invoicing</h1>

      {/* Tabs */}
      <div className="flex bg-gray-100 rounded-xl p-1 mb-4">
        {([['create', 'Create Invoice'], ['history', 'History']] as const).map(([k, label]) => (
          <button key={k} onClick={() => setTab(k as 'create' | 'history')}
            className={`flex-1 py-2 text-sm font-semibold rounded-lg transition-all ${
              tab === k ? 'bg-white shadow-sm' : 'text-gray-500'
            }`}
            style={tab === k ? { color: tenant.accentColor } : undefined}
          >
            {label}
          </button>
        ))}
      </div>

      {tab === 'create' && (
        <>
          <p className="text-sm text-gray-500 mb-3">Select completed runs to include in your invoice.</p>
          {lines.length === 0 && (
            <div className="text-center text-gray-400 py-12">
              <div className="text-4xl mb-2">📄</div>
              No uninvoiced runs
            </div>
          )}
          {lines.map(line => (
            <button key={line.id} onClick={() => toggleLine(line.id)}
              className={`w-full text-left bg-white rounded-xl p-4 shadow-sm border mb-2 transition-all ${
                line.selected ? 'border-2' : 'border-gray-100'
              }`}
              style={line.selected ? { borderColor: tenant.accentColor } : undefined}
            >
              <div className="flex justify-between items-start">
                <div className="flex-1 min-w-0">
                  <div className="font-semibold text-sm text-gray-900">{line.runName} – {line.date}</div>
                  <div className="text-xs text-gray-500 mt-0.5">{line.description}</div>
                </div>
                <div className="flex items-center gap-2 shrink-0 ml-3">
                  <span className="font-bold text-sm" style={{ color: tenant.accentColor }}>${line.amount.toFixed(2)}</span>
                  <div className={`w-5 h-5 rounded-md border-2 flex items-center justify-center text-xs ${
                    line.selected ? 'text-white' : 'border-gray-300'
                  }`} style={line.selected ? { background: tenant.accentColor, borderColor: tenant.accentColor } : undefined}>
                    {line.selected && '✓'}
                  </div>
                </div>
              </div>
            </button>
          ))}

          {selectedLines.length > 0 && (
            <div className="bg-white rounded-2xl p-4 shadow-sm border border-gray-100 mt-4">
              <div className="flex justify-between text-sm mb-1">
                <span className="text-gray-500">Subtotal</span>
                <span className="font-medium">${subtotal.toFixed(2)}</span>
              </div>
              <div className="flex justify-between text-sm mb-1">
                <span className="text-gray-500">{taxLabel}</span>
                <span className="font-medium">${tax.toFixed(2)}</span>
              </div>
              <div className="border-t border-gray-200 my-2" />
              <div className="flex justify-between text-base font-bold">
                <span>Total</span>
                <span style={{ color: tenant.accentColor }}>${total.toFixed(2)}</span>
              </div>
              <button onClick={handleSubmit}
                className="w-full mt-3 py-3 rounded-xl text-sm font-semibold text-white active:scale-[0.97] transition-transform"
                style={{ background: tenant.accentColor }}>
                Submit Invoice
              </button>
            </div>
          )}
        </>
      )}

      {tab === 'history' && (
        <>
          {MOCK_INVOICES.map(inv => (
            <div key={inv.id} className="bg-white rounded-xl p-4 shadow-sm border border-gray-100 mb-3">
              <div className="flex justify-between items-start">
                <div>
                  <div className="font-semibold text-sm text-gray-900">{inv.invoiceNumber}</div>
                  <div className="text-xs text-gray-400">{inv.date}</div>
                </div>
                <div className="text-right">
                  <div className="font-bold text-sm" style={{ color: tenant.accentColor }}>${inv.total.toFixed(2)}</div>
                  <span className={`text-xs font-medium px-2 py-0.5 rounded-full ${
                    inv.status === 'Paid' ? 'bg-green-50 text-green-700' : 'bg-amber-50 text-amber-700'
                  }`}>{inv.status}</span>
                </div>
              </div>
              <div className="mt-2 text-xs text-gray-500 flex justify-between">
                <span>Subtotal: ${inv.subtotal.toFixed(2)}</span>
                <span>{taxLabel}: ${inv.tax.toFixed(2)}</span>
              </div>
            </div>
          ))}
        </>
      )}
    </div>
  );
}
