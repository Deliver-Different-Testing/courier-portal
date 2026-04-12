import { useState } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import { resolveTenant, DEFAULT_DOC_REQUIREMENTS } from '@/lib/tenants';

// ── Mock compliance data (would come from API) ──
interface ComplianceAlert {
  docTypeId: number;
  docTypeName: string;
  status: 'expired' | 'expiring' | 'missing';
  expiryDate?: string;
  daysRemaining?: number;
}

function getComplianceAlerts(country: 'NZ' | 'US'): ComplianceAlert[] {
  // Mock — in production this comes from GET /api/courier/me/compliance
  return [
    { docTypeId: 3, docTypeName: 'Vehicle Insurance', status: 'expired', expiryDate: '2026-02-28', daysRemaining: -7 },
    { docTypeId: 2, docTypeName: 'Vehicle Registration', status: 'expiring', expiryDate: '2026-04-10', daysRemaining: 34 },
  ];
}

interface StatCard {
  label: string;
  value: string;
  sub?: string;
  icon: string;
  color: string;
}

export default function CourierDashboard() {
  const { tenantSlug } = useParams<{ tenantSlug: string }>();
  const tenant = resolveTenant(tenantSlug);
  const navigate = useNavigate();

  const alerts = getComplianceAlerts(tenant.country);
  const expired = alerts.filter(a => a.status === 'expired' || a.status === 'missing');
  const expiring = alerts.filter(a => a.status === 'expiring');
  const hasExpired = expired.length > 0;

  // Dismiss state — courier can dismiss the warning banner but not the blocking modal
  const [dismissedWarning, setDismissedWarning] = useState(false);
  // Block modal dismissed this session (they acknowledged but can't work until resolved)
  const [acknowledgedBlock, setAcknowledgedBlock] = useState(false);

  const complianceStatus = hasExpired ? 'Non-Compliant' : expiring.length > 0 ? 'At Risk' : 'Active';
  const complianceSub = hasExpired
    ? `${expired.length} expired — action required`
    : expiring.length > 0
    ? `${expiring.length} doc${expiring.length > 1 ? 's' : ''} expiring soon`
    : 'All docs current';
  const complianceColor = hasExpired ? '#dc2626' : expiring.length > 0 ? '#d97706' : '#6366f1';

  const stats: StatCard[] = [
    { label: "Today's Deliveries", value: hasExpired ? '—' : '12', sub: hasExpired ? 'Suspended' : '3 remaining', icon: '📦', color: hasExpired ? '#9ca3af' : tenant.accentColor },
    { label: "This Week's Earnings", value: '$1,245.80', sub: '23 runs completed', icon: '💰', color: '#10b981' },
    { label: 'Compliance', value: complianceStatus, sub: complianceSub, icon: hasExpired ? '🚫' : expiring.length > 0 ? '⚠️' : '✅', color: complianceColor },
    { label: 'Next Run', value: hasExpired ? 'Suspended' : 'Tomorrow', sub: hasExpired ? 'Resolve compliance first' : 'Auckland Central, 8:00 AM', icon: '🗓️', color: hasExpired ? '#9ca3af' : '#f59e0b' },
  ];

  return (
    <div className="px-4 pt-4 relative">

      {/* ══════ BLOCKING MODAL — expired/missing docs ══════ */}
      {hasExpired && !acknowledgedBlock && (
        <div className="fixed inset-0 z-[100] bg-black/60 flex items-end sm:items-center justify-center">
          <div className="bg-white w-full max-w-md rounded-t-3xl sm:rounded-2xl p-6 pb-8 animate-slide-up">
            <div className="text-center mb-4">
              <div className="w-16 h-16 rounded-full bg-red-100 flex items-center justify-center mx-auto mb-3">
                <span className="text-3xl">🚫</span>
              </div>
              <h2 className="text-lg font-bold text-gray-900">Compliance Action Required</h2>
              <p className="text-sm text-gray-500 mt-1">
                You have expired or missing documents. You are <span className="font-semibold text-red-600">ineligible for work</span> until these are resolved.
              </p>
            </div>

            <div className="space-y-2 mb-5">
              {expired.map(a => (
                <div key={a.docTypeId} className="flex items-center gap-3 bg-red-50 border border-red-200 rounded-xl px-4 py-3">
                  <span className="text-lg">{a.status === 'expired' ? '❌' : '📄'}</span>
                  <div className="flex-1">
                    <div className="text-sm font-semibold text-red-800">{a.docTypeName}</div>
                    <div className="text-xs text-red-600">
                      {a.status === 'expired'
                        ? `Expired ${a.expiryDate ? new Date(a.expiryDate).toLocaleDateString('en-NZ', { day: 'numeric', month: 'short', year: 'numeric' }) : ''}`
                        : 'Not uploaded — required'
                      }
                    </div>
                  </div>
                </div>
              ))}
            </div>

            <button
              onClick={() => { setAcknowledgedBlock(true); navigate(`/courier/${tenant.slug}/documents`); }}
              className="w-full py-3.5 rounded-xl text-white font-semibold text-sm bg-red-600 active:bg-red-700 mb-2"
            >
              📤 Upload Documents Now
            </button>
            <button
              onClick={() => setAcknowledgedBlock(true)}
              className="w-full py-2.5 text-sm text-gray-500 font-medium"
            >
              I understand — continue to dashboard
            </button>
            <p className="text-[11px] text-gray-400 text-center mt-2">
              You will not be assigned new runs until all required documents are current.
            </p>
          </div>
        </div>
      )}

      {/* ══════ WARNING BANNER — expiring docs (not yet expired) ══════ */}
      {!hasExpired && expiring.length > 0 && !dismissedWarning && (
        <div className="bg-amber-50 border border-amber-200 rounded-2xl px-4 py-3.5 mb-4 relative">
          <button
            onClick={() => setDismissedWarning(true)}
            className="absolute top-2 right-2 text-amber-400 hover:text-amber-600 text-lg leading-none"
          >
            ×
          </button>
          <div className="flex items-start gap-3">
            <span className="text-xl mt-0.5">⚠️</span>
            <div className="flex-1">
              <div className="text-sm font-semibold text-amber-800">Documents Expiring Soon</div>
              <p className="text-xs text-amber-700 mt-0.5 mb-2">
                Upload renewed documents before they expire to stay eligible for work.
              </p>
              {expiring.map(a => (
                <div key={a.docTypeId} className="flex items-center gap-2 text-xs text-amber-700 py-1">
                  <span className="font-medium">{a.docTypeName}</span>
                  <span className="text-amber-500">—</span>
                  <span>
                    expires {a.expiryDate ? new Date(a.expiryDate).toLocaleDateString('en-NZ', { day: 'numeric', month: 'short' }) : 'soon'}
                    {a.daysRemaining != null && ` (${a.daysRemaining} days)`}
                  </span>
                </div>
              ))}
              <button
                onClick={() => navigate(`/courier/${tenant.slug}/documents`)}
                className="mt-2 text-xs font-semibold px-4 py-2 rounded-lg text-white"
                style={{ backgroundColor: tenant.accentColor }}
              >
                Update Documents →
              </button>
            </div>
          </div>
        </div>
      )}

      {/* ══════ PERSISTENT TOP BAR — when expired and modal dismissed ══════ */}
      {hasExpired && acknowledgedBlock && (
        <div
          className="bg-red-600 text-white rounded-2xl px-4 py-3 mb-4 flex items-center gap-3 cursor-pointer active:bg-red-700"
          onClick={() => navigate(`/courier/${tenant.slug}/documents`)}
        >
          <span className="text-lg">🚫</span>
          <div className="flex-1">
            <div className="text-sm font-semibold">You are ineligible for work</div>
            <div className="text-xs text-red-200">{expired.length} document{expired.length > 1 ? 's' : ''} expired or missing — tap to resolve</div>
          </div>
          <span className="text-red-200">›</span>
        </div>
      )}

      <h1 className="text-xl font-bold text-gray-900 mb-1">Dashboard</h1>
      <p className="text-sm text-gray-500 mb-4">Welcome back, Courier</p>

      <div className="grid grid-cols-2 gap-3">
        {stats.map(s => (
          <div
            key={s.label}
            className={`bg-white rounded-2xl p-4 shadow-sm border border-gray-100 ${s.label === 'Compliance' ? 'cursor-pointer active:scale-[0.98] transition-transform' : ''}`}
            onClick={s.label === 'Compliance' ? () => navigate(`/courier/${tenant.slug}/documents`) : undefined}
          >
            <div className="flex items-center gap-2 mb-2">
              <span className="text-xl">{s.icon}</span>
              <span className="text-xs font-medium text-gray-500 leading-tight">{s.label}</span>
            </div>
            <div className="text-lg font-bold" style={{ color: s.color }}>{s.value}</div>
            {s.sub && <div className="text-xs text-gray-400 mt-0.5">{s.sub}</div>}
          </div>
        ))}
      </div>

      {/* Quick Actions */}
      <div className="mt-6">
        <h2 className="text-sm font-semibold text-gray-700 mb-3">Quick Actions</h2>
        <div className="flex gap-3">
          {[
            { label: 'View Runs', icon: '🚚', path: 'runs' },
            { label: 'My Docs', icon: '📄', path: 'documents' },
            { label: 'Schedule', icon: '📅', path: 'schedule' },
            { label: 'Invoices', icon: '💰', path: 'invoicing' },
          ].map(a => (
            <a key={a.path} href={`#/courier/${tenant.slug}/${a.path}`}
               className="flex-1 bg-white rounded-xl p-3 text-center shadow-sm border border-gray-100 active:scale-[0.97] transition-transform">
              <span className="text-2xl block mb-1">{a.icon}</span>
              <span className="text-xs font-medium text-gray-700">{a.label}</span>
            </a>
          ))}
        </div>
      </div>
    </div>
  );
}
