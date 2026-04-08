import { useState, useEffect, useCallback, useRef } from 'react';

// ─── Types ───
type LogCategory = 'invitation' | 'contractor' | 'webhook' | 'auth';
type LogDirection = 'Inbound' | 'Outbound';
type Filter = 'all' | 'invitation' | 'contractor' | 'webhook' | 'errors';

interface RecruitmentLogEntry {
  id: string;
  timestamp: string;
  direction: LogDirection;
  endpoint: string;
  status: number;
  success: boolean;
  durationMs: number;
  responseSummary: string;
  category: LogCategory;
}

// ─── Mock Data (recruitment-focused) ───
function generateMockRecruitmentLog(): RecruitmentLogEntry[] {
  const now = Date.now();
  return [
    { id: 'r1', timestamp: new Date(now - 180000).toISOString(), direction: 'Outbound', endpoint: 'POST /api/Openforce/Invitations/Send', status: 200, success: true, durationMs: 312, responseSummary: 'Invitation sent to john.driver@email.com — activation code IC', category: 'invitation' },
    { id: 'r2', timestamp: new Date(now - 600000).toISOString(), direction: 'Outbound', endpoint: 'POST /api/Openforce/Contractors/Search', status: 200, success: true, durationMs: 245, responseSummary: '3 contractors found matching "Smith" in Atlanta region', category: 'contractor' },
    { id: 'r3', timestamp: new Date(now - 1200000).toISOString(), direction: 'Inbound', endpoint: 'POST /webhook/openforce/contract_activated', status: 200, success: true, durationMs: 42, responseSummary: 'Contract activated for contractor #4521 — IC agreement signed', category: 'webhook' },
    { id: 'r4', timestamp: new Date(now - 2100000).toISOString(), direction: 'Outbound', endpoint: 'POST /api/Openforce/Auth/Token', status: 200, success: true, durationMs: 189, responseSummary: 'Auth token refreshed — expires in 3600s', category: 'auth' },
    { id: 'r5', timestamp: new Date(now - 3600000).toISOString(), direction: 'Outbound', endpoint: 'POST /api/Openforce/Invitations/Send', status: 200, success: true, durationMs: 298, responseSummary: 'Invitation sent to maria.courier@email.com — master code', category: 'invitation' },
    { id: 'r6', timestamp: new Date(now - 5400000).toISOString(), direction: 'Outbound', endpoint: 'POST /api/Openforce/Invitations/Send', status: 422, success: false, durationMs: 134, responseSummary: 'Validation error: email already registered in Openforce', category: 'invitation' },
    { id: 'r7', timestamp: new Date(now - 7200000).toISOString(), direction: 'Inbound', endpoint: 'POST /webhook/openforce/contract_activated', status: 200, success: true, durationMs: 38, responseSummary: 'Contract activated for contractor #4518 — subcontractor agreement', category: 'webhook' },
    { id: 'r8', timestamp: new Date(now - 10800000).toISOString(), direction: 'Outbound', endpoint: 'POST /api/Openforce/Contractors/Search', status: 200, success: true, durationMs: 356, responseSummary: '7 contractors found matching "Johnson" — nationwide', category: 'contractor' },
    { id: 'r9', timestamp: new Date(now - 14400000).toISOString(), direction: 'Outbound', endpoint: 'POST /api/Openforce/Invitations/Send', status: 200, success: true, durationMs: 275, responseSummary: 'Bulk invitation — 5 contractors invited for Dallas region', category: 'invitation' },
    { id: 'r10', timestamp: new Date(now - 18000000).toISOString(), direction: 'Outbound', endpoint: 'POST /api/Openforce/Auth/Token', status: 401, success: false, durationMs: 89, responseSummary: 'Auth failed — invalid API key (check Accounts config)', category: 'auth' },
    { id: 'r11', timestamp: new Date(now - 21600000).toISOString(), direction: 'Inbound', endpoint: 'POST /webhook/openforce/contract_activated', status: 200, success: true, durationMs: 51, responseSummary: 'Contract activated for contractor #4515 — IC agreement signed', category: 'webhook' },
    { id: 'r12', timestamp: new Date(now - 28800000).toISOString(), direction: 'Outbound', endpoint: 'POST /api/Openforce/Contractors/Search', status: 500, success: false, durationMs: 2015, responseSummary: 'Openforce API timeout — service unavailable', category: 'contractor' },
    { id: 'r13', timestamp: new Date(now - 36000000).toISOString(), direction: 'Outbound', endpoint: 'POST /api/Openforce/Invitations/Send', status: 200, success: true, durationMs: 267, responseSummary: 'Invitation sent to alex.driver@email.com — IC code', category: 'invitation' },
  ];
}

function statusColor(status: number) {
  if (status >= 200 && status < 300) return 'text-emerald-600 bg-emerald-50';
  if (status >= 400 && status < 500) return 'text-amber-600 bg-amber-50';
  return 'text-red-600 bg-red-50';
}

export default function OpenforceActivity() {
  const [connected, setConnected] = useState<boolean | null>(null);
  const [log] = useState<RecruitmentLogEntry[]>(() => generateMockRecruitmentLog());
  const [filter, setFilter] = useState<Filter>('all');
  const [autoRefresh, setAutoRefresh] = useState(false);
  const timerRef = useRef<ReturnType<typeof setInterval> | null>(null);

  // Check connection status
  useEffect(() => {
    (async () => {
      try {
        const res = await fetch('/api/Openforce/Connected');
        setConnected(res.ok ? await res.json() : false);
      } catch {
        setConnected(false);
      }
    })();
  }, []);

  useEffect(() => {
    if (autoRefresh) {
      timerRef.current = setInterval(() => {}, 30000);
      return () => { if (timerRef.current) clearInterval(timerRef.current); };
    } else {
      if (timerRef.current) clearInterval(timerRef.current);
    }
  }, [autoRefresh]);

  const filtered = log.filter(e => {
    if (filter === 'all') return true;
    if (filter === 'errors') return !e.success;
    if (filter === 'invitation') return e.category === 'invitation';
    if (filter === 'contractor') return e.category === 'contractor';
    if (filter === 'webhook') return e.category === 'webhook';
    return true;
  });

  const now = Date.now();
  const last24h = log.filter(e => now - new Date(e.timestamp).getTime() < 86400000);
  const successRate = last24h.length ? Math.round((last24h.filter(e => e.success).length / last24h.length) * 100) : 0;
  const avgDuration = last24h.length ? Math.round(last24h.reduce((s, e) => s + e.durationMs, 0) / last24h.length) : 0;
  const lastSuccess = log.find(e => e.success);
  const lastError = log.find(e => !e.success);

  const filters: [Filter, string][] = [['all', 'All'], ['invitation', 'Invitations'], ['contractor', 'Contractors'], ['webhook', 'Webhooks'], ['errors', 'Errors']];

  return (
    <div className="space-y-6 max-w-6xl">
      {/* Header */}
      <div>
        <h1 className="text-2xl font-bold text-[#0d0c2c]">🔗 Openforce Recruitment Activity</h1>
        <p className="text-sm text-gray-500 mt-1">Monitor recruitment and contractor onboarding API activity</p>
      </div>

      {/* Connection Status + Config Note */}
      <div className="bg-white rounded-xl border border-gray-200 shadow-sm p-6">
        <div className="flex items-center justify-between">
          <div className="flex items-center gap-3">
            <span className="text-sm font-medium text-[#0d0c2c]">Connection Status</span>
            {connected === null && <span className="inline-flex items-center gap-1.5 text-xs font-medium text-amber-600"><span className="w-2 h-2 rounded-full bg-amber-400" />Checking…</span>}
            {connected === true && <span className="inline-flex items-center gap-1.5 text-xs font-medium text-emerald-600"><span className="w-2 h-2 rounded-full bg-emerald-500 animate-pulse" />Connected ✅</span>}
            {connected === false && <span className="inline-flex items-center gap-1.5 text-xs font-medium text-red-600"><span className="w-2 h-2 rounded-full bg-red-500" />Not Connected ❌</span>}
          </div>
        </div>
        <div className="mt-3 px-3 py-2 bg-[#f4f2f1] rounded-lg">
          <p className="text-xs text-gray-500">
            ℹ️ Configuration managed in <strong>Accounts → Contractor Settlements → Openforce</strong>. This view is read-only.
          </p>
        </div>
      </div>

      {/* Activity Log */}
      <div className="bg-white rounded-xl border border-gray-200 shadow-sm">
        <div className="p-6 border-b border-gray-100">
          <div className="flex items-center justify-between">
            <h2 className="text-lg font-semibold text-[#0d0c2c]">Recruitment API Activity</h2>
            <label className="flex items-center gap-2 text-sm text-gray-500 cursor-pointer select-none">
              <input type="checkbox" checked={autoRefresh} onChange={e => setAutoRefresh(e.target.checked)} className="rounded border-gray-300 text-[#3bc7f4] focus:ring-[#3bc7f4]" />
              Auto-refresh (30s)
            </label>
          </div>
        </div>

        {/* Stats bar */}
        <div className="grid grid-cols-2 md:grid-cols-5 gap-4 p-6 border-b border-gray-100 bg-[#f4f2f1]/50">
          <div>
            <div className="text-xs text-gray-400 uppercase tracking-wide">Calls (24h)</div>
            <div className="text-xl font-bold text-[#0d0c2c]">{last24h.length}</div>
          </div>
          <div>
            <div className="text-xs text-gray-400 uppercase tracking-wide">Success Rate</div>
            <div className={`text-xl font-bold ${successRate >= 90 ? 'text-emerald-600' : successRate >= 70 ? 'text-amber-600' : 'text-red-600'}`}>{successRate}%</div>
          </div>
          <div>
            <div className="text-xs text-gray-400 uppercase tracking-wide">Avg Response</div>
            <div className="text-xl font-bold text-[#0d0c2c]">{avgDuration}ms</div>
          </div>
          <div>
            <div className="text-xs text-gray-400 uppercase tracking-wide">Last Success</div>
            <div className="text-sm font-medium text-emerald-600">{lastSuccess ? new Date(lastSuccess.timestamp).toLocaleString() : '—'}</div>
          </div>
          <div>
            <div className="text-xs text-gray-400 uppercase tracking-wide">Last Error</div>
            <div className="text-sm font-medium text-red-600">{lastError ? new Date(lastError.timestamp).toLocaleString() : '—'}</div>
          </div>
        </div>

        {/* Filter pills */}
        <div className="px-6 py-3 border-b border-gray-100 flex items-center gap-2 flex-wrap">
          {filters.map(([key, label]) => (
            <button key={key} onClick={() => setFilter(key)} className={`px-3 py-1.5 rounded-full text-xs font-medium transition-colors ${filter === key ? 'bg-[#3bc7f4] text-white' : 'bg-gray-100 text-gray-600 hover:bg-gray-200'}`}>
              {label}
            </button>
          ))}
        </div>

        {/* Log table */}
        <div className="overflow-auto" style={{ maxHeight: 400 }}>
          {filtered.length === 0 ? (
            <div className="flex flex-col items-center justify-center py-16 text-gray-400">
              <p className="text-sm font-medium">No activity matching this filter</p>
              <p className="text-xs mt-1">Recruitment API calls will appear here</p>
            </div>
          ) : (
            <table className="w-full text-sm">
              <thead className="bg-[#f4f2f1] sticky top-0 z-10">
                <tr>
                  <th className="text-left px-4 py-2.5 text-xs font-medium text-gray-500 uppercase">Timestamp</th>
                  <th className="text-left px-4 py-2.5 text-xs font-medium text-gray-500 uppercase">Direction</th>
                  <th className="text-left px-4 py-2.5 text-xs font-medium text-gray-500 uppercase">Endpoint</th>
                  <th className="text-center px-4 py-2.5 text-xs font-medium text-gray-500 uppercase">Status</th>
                  <th className="text-right px-4 py-2.5 text-xs font-medium text-gray-500 uppercase">Duration</th>
                  <th className="text-left px-4 py-2.5 text-xs font-medium text-gray-500 uppercase">Response</th>
                </tr>
              </thead>
              <tbody className="divide-y divide-gray-50">
                {filtered.map(entry => (
                  <tr key={entry.id} className="hover:bg-gray-50/50">
                    <td className="px-4 py-2.5 text-xs text-gray-500 whitespace-nowrap">{new Date(entry.timestamp).toLocaleString()}</td>
                    <td className="px-4 py-2.5">
                      <span className={`inline-flex items-center gap-1 text-xs font-medium ${entry.direction === 'Inbound' ? 'text-purple-600' : 'text-blue-600'}`}>
                        {entry.direction === 'Inbound' ? '↙' : '↗'} {entry.direction}
                      </span>
                    </td>
                    <td className="px-4 py-2.5 text-xs font-mono text-gray-700 max-w-[260px] truncate">{entry.endpoint}</td>
                    <td className="px-4 py-2.5 text-center">
                      <span className={`inline-block px-2 py-0.5 rounded-full text-xs font-medium ${statusColor(entry.status)}`}>
                        {entry.success ? '✅' : '❌'} {entry.status}
                      </span>
                    </td>
                    <td className="px-4 py-2.5 text-xs text-gray-500 text-right whitespace-nowrap">{entry.durationMs.toLocaleString()}ms</td>
                    <td className="px-4 py-2.5 text-xs text-gray-600 max-w-[240px] truncate">{entry.responseSummary}</td>
                  </tr>
                ))}
              </tbody>
            </table>
          )}
        </div>
      </div>
    </div>
  );
}
