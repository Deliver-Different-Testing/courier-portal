import { useState, useRef, TouchEvent } from 'react';
import { useParams } from 'react-router-dom';
import { resolveTenant } from '@/lib/tenants';

// ── Types ──

interface Job {
  jobId: number;
  jobNumber: string;
  clientCode: string;
  deliveryAddress: string;
  status: string;
  void: boolean;
  courierPayment: number | null;
  masterPayment: number | null;
  courierFuel: number | null;
  masterFuel: number | null;
}

interface Run {
  bookDate: string;
  runName: string;
  dateDisplay: string;
  states: string;
  cities: string;
  amount: number | null;
  masterAmount: number | null;
  jobs: Job[];
}

// ── Mock Data ──

const MOCK_JOBS: Job[] = [
  { jobId: 1, jobNumber: 'J-10234', clientCode: 'ACME', deliveryAddress: '42 Queen St, Auckland CBD', status: 'Delivered', void: false, courierPayment: 12.50, masterPayment: null, courierFuel: 2.10, masterFuel: null },
  { jobId: 2, jobNumber: 'J-10235', clientCode: 'KIWI', deliveryAddress: '88 Symonds St, Grafton', status: 'Delivered', void: false, courierPayment: 8.75, masterPayment: null, courierFuel: 1.40, masterFuel: null },
  { jobId: 3, jobNumber: 'J-10236', clientCode: 'FAST', deliveryAddress: '12 Broadway, Newmarket', status: 'In Transit', void: false, courierPayment: 15.00, masterPayment: 18.00, courierFuel: 3.00, masterFuel: 3.60 },
];

const MOCK_RUNS: { current: Run[]; past: Run[] } = {
  current: [
    { bookDate: '2026-03-07', runName: 'AKL-AM', dateDisplay: 'Sat 7 Mar', states: 'Auckland', cities: 'CBD, Grafton, Newmarket', amount: 245.50, masterAmount: null, jobs: MOCK_JOBS },
    { bookDate: '2026-03-08', runName: 'AKL-PM', dateDisplay: 'Sun 8 Mar', states: 'Auckland', cities: 'Ponsonby, Grey Lynn', amount: 180.00, masterAmount: 220.00, jobs: MOCK_JOBS.slice(0, 2) },
  ],
  past: [
    { bookDate: '2026-03-05', runName: 'AKL-AM', dateDisplay: 'Thu 5 Mar', states: 'Auckland', cities: 'CBD, Mt Eden', amount: 310.25, masterAmount: null, jobs: MOCK_JOBS },
    { bookDate: '2026-03-04', runName: 'AKL-PM', dateDisplay: 'Wed 4 Mar', states: 'Auckland', cities: 'Penrose, Onehunga', amount: 195.00, masterAmount: null, jobs: MOCK_JOBS.slice(0, 1) },
  ],
};

// ── Swipeable Run Card ──

function RunCard({ run, tenant, onCancel, onClick }: {
  run: Run; tenant: ReturnType<typeof resolveTenant>;
  onCancel: () => void; onClick: () => void;
}) {
  const startX = useRef(0);
  const [offsetX, setOffsetX] = useState(0);
  const [swiped, setSwiped] = useState(false);

  const onTouchStart = (e: TouchEvent) => { startX.current = e.touches[0].clientX; };
  const onTouchMove = (e: TouchEvent) => {
    const diff = startX.current - e.touches[0].clientX;
    if (diff > 0) setOffsetX(Math.min(diff, 100));
  };
  const onTouchEnd = () => {
    if (offsetX > 60) { setSwiped(true); setOffsetX(100); }
    else { setOffsetX(0); }
  };

  const truncate = (s: string, n: number) => s.length > n ? s.substring(0, n - 3) + '…' : s;

  return (
    <div className="relative overflow-hidden rounded-xl mb-3">
      {/* Cancel button behind */}
      <div className="absolute inset-y-0 right-0 w-24 bg-red-500 flex items-center justify-center text-white font-semibold text-sm"
           onClick={(e) => { e.stopPropagation(); onCancel(); }}>
        CANCEL
      </div>
      {/* Card */}
      <div
        className="relative bg-white border border-gray-100 shadow-sm rounded-xl p-4 transition-transform active:bg-gray-50"
        style={{ transform: `translateX(-${swiped ? 100 : offsetX}px)` }}
        onTouchStart={onTouchStart} onTouchMove={onTouchMove} onTouchEnd={onTouchEnd}
        onClick={onClick}
      >
        <div className="flex items-start justify-between">
          <div className="flex-1 min-w-0">
            <div className="font-semibold text-gray-900 text-sm">{run.dateDisplay}</div>
            <div className="text-xs text-gray-500 mt-0.5">{truncate(run.states, 20)}</div>
          </div>
          <div className="text-center px-3">
            <div className="text-xs text-gray-500">{run.jobs.length} Stops</div>
            <div className="text-xs text-gray-400 mt-0.5">{truncate(run.cities, 20)}</div>
          </div>
          <div className="text-right shrink-0">
            {run.amount != null ? (
              <span className="font-bold text-sm" style={{ color: tenant.accentColor }}>
                ${run.amount.toFixed(2)}
              </span>
            ) : (
              <span className="text-xs text-gray-400">N/A</span>
            )}
            {run.masterAmount != null && (
              <div className="text-xs text-gray-400">(${run.masterAmount.toFixed(2)})</div>
            )}
          </div>
        </div>
      </div>
    </div>
  );
}

// ── Run Detail ──

function RunDetail({ run, tenant, onBack }: {
  run: Run; tenant: ReturnType<typeof resolveTenant>; onBack: () => void;
}) {
  const [limit, setLimit] = useState(20);

  return (
    <div className="px-4 pt-4">
      <button onClick={onBack} className="flex items-center text-sm font-medium mb-4" style={{ color: tenant.accentColor }}>
        ← Back to Runs
      </button>
      <div className="bg-white rounded-2xl p-4 shadow-sm border border-gray-100 mb-4">
        <div className="flex justify-between items-start">
          <div>
            <div className="font-bold text-gray-900">{run.dateDisplay}</div>
            <div className="text-sm text-gray-500">{run.cities}</div>
          </div>
          <div className="text-right">
            <div className="text-sm text-gray-500">{run.jobs.length} Stops</div>
            {run.amount != null && (
              <div className="font-bold" style={{ color: tenant.accentColor }}>${run.amount.toFixed(2)}</div>
            )}
            {run.masterAmount != null && (
              <div className="text-xs text-gray-400">(${run.masterAmount.toFixed(2)})</div>
            )}
          </div>
        </div>
      </div>

      <h3 className="text-sm font-semibold text-gray-700 mb-2">Jobs</h3>
      <div className="space-y-2">
        {run.jobs.slice(0, limit).map(job => (
          <div key={job.jobId} className="bg-white rounded-xl p-3 shadow-sm border border-gray-100">
            <div className="flex justify-between items-start">
              <div className="flex-1 min-w-0">
                <div className="flex items-center gap-2">
                  <span className="font-mono text-xs font-semibold text-gray-700">{job.jobNumber}</span>
                  <span className="text-xs text-gray-400">{job.clientCode}</span>
                </div>
                <div className="text-sm text-gray-600 mt-1">{job.deliveryAddress}</div>
              </div>
              <div className="text-right shrink-0 ml-3">
                <span className={`text-xs font-medium px-2 py-0.5 rounded-full ${
                  job.void ? 'bg-gray-100 text-gray-500' :
                  job.status === 'Delivered' ? 'bg-green-50 text-green-700' :
                  'bg-blue-50 text-blue-700'
                }`}>
                  {job.void ? 'Void' : job.status}
                </span>
                {job.courierPayment != null && (
                  <div className="text-xs text-gray-500 mt-1">
                    Pay: ${job.courierPayment.toFixed(2)}
                    {job.masterPayment != null && <span className="text-gray-400"> (${job.masterPayment.toFixed(2)})</span>}
                  </div>
                )}
                {job.courierFuel != null && (
                  <div className="text-xs text-gray-400">
                    Fuel: ${job.courierFuel.toFixed(2)}
                    {job.masterFuel != null && <span> (${job.masterFuel.toFixed(2)})</span>}
                  </div>
                )}
              </div>
            </div>
          </div>
        ))}
      </div>
      {limit < run.jobs.length && (
        <button onClick={() => setLimit(l => l + 20)}
                className="w-full text-center text-sm font-medium py-3 mt-2" style={{ color: tenant.accentColor }}>
          Show More…
        </button>
      )}
    </div>
  );
}

// ── Main Component ──

export default function MyRuns() {
  const { tenantSlug } = useParams<{ tenantSlug: string }>();
  const tenant = resolveTenant(tenantSlug);
  const [tab, setTab] = useState<'current' | 'past'>('current');
  const [selectedRun, setSelectedRun] = useState<Run | null>(null);
  const [pastLimit, setPastLimit] = useState(10);

  const runs = MOCK_RUNS;

  if (selectedRun) {
    return <RunDetail run={selectedRun} tenant={tenant} onBack={() => setSelectedRun(null)} />;
  }

  return (
    <div className="px-4 pt-4">
      <h1 className="text-xl font-bold text-gray-900 mb-4">My Runs</h1>

      {/* Tabs */}
      <div className="flex bg-gray-100 rounded-xl p-1 mb-4">
        {(['current', 'past'] as const).map(t => (
          <button key={t} onClick={() => setTab(t)}
            className={`flex-1 py-2 text-sm font-semibold rounded-lg transition-all ${
              tab === t ? 'bg-white shadow-sm text-gray-900' : 'text-gray-500'
            }`}
            style={tab === t ? { color: tenant.accentColor } : undefined}
          >
            {t === 'current' ? 'Current Runs' : 'Past Runs'}
          </button>
        ))}
      </div>

      {/* Run List */}
      {tab === 'current' && runs.current.map((run, i) => (
        <RunCard key={i} run={run} tenant={tenant}
          onCancel={() => alert(`Cancel run ${run.runName} on ${run.bookDate}?`)}
          onClick={() => setSelectedRun(run)} />
      ))}

      {tab === 'past' && (
        <>
          {runs.past.slice(0, pastLimit).map((run, i) => (
            <RunCard key={i} run={run} tenant={tenant}
              onCancel={() => {}} onClick={() => setSelectedRun(run)} />
          ))}
          {pastLimit < runs.past.length && (
            <button onClick={() => setPastLimit(l => l + 10)}
                    className="w-full text-center text-sm font-medium py-3" style={{ color: tenant.accentColor }}>
              Show More…
            </button>
          )}
        </>
      )}

      {runs[tab].length === 0 && (
        <div className="text-center text-gray-400 py-12">
          <div className="text-4xl mb-2">📭</div>
          No {tab} runs
        </div>
      )}
    </div>
  );
}
