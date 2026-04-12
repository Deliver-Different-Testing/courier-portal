import { useState, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import { portalCourierService, type CourierProfile, type Run, type Schedule } from '@/services/portal_courierService';
import { getUninvoiced, type UninvoicedData } from '@/services/portal_invoiceService';

export default function PortalDashboard() {
  const navigate = useNavigate();

  const [profile, setProfile] = useState<CourierProfile | null>(null);
  const [runs, setRuns] = useState<Run[]>([]);
  const [schedules, setSchedules] = useState<Schedule[]>([]);
  const [uninvoiced, setUninvoiced] = useState<UninvoicedData | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    let cancelled = false;
    async function load() {
      setLoading(true);
      setError(null);
      try {
        const [profileData, runsData, schedulesData, uninvoicedData] = await Promise.all([
          portalCourierService.getProfile(),
          portalCourierService.getRuns(),
          portalCourierService.getSchedules(),
          getUninvoiced().catch(() => null),
        ]);
        if (!cancelled) {
          setProfile(profileData);
          setRuns(runsData);
          setSchedules(schedulesData);
          setUninvoiced(uninvoicedData);
        }
      } catch (err) {
        if (!cancelled) setError('Failed to load dashboard data.');
        console.error('Dashboard load failed:', err);
      } finally {
        if (!cancelled) setLoading(false);
      }
    }
    load();
    return () => { cancelled = true; };
  }, []);

  const upcomingRuns = runs.filter(r => r.status !== 'Completed').length;
  const pendingSchedules = schedules.filter(s => !s.response).length;

  if (loading) {
    return (
      <div className="flex items-center justify-center py-16">
        <div className="text-text-muted text-sm">Loading…</div>
      </div>
    );
  }

  if (error) {
    return (
      <div className="bg-error/10 border border-error/30 rounded-xl p-4 text-error text-sm">
        {error}
      </div>
    );
  }

  return (
    <div className="space-y-4">
      {/* Welcome */}
      <div className="bg-brand-dark rounded-xl p-4 text-white">
        <p className="text-white/60 text-xs">Welcome back</p>
        <h1 className="text-lg font-bold">
          {profile ? `${profile.firstName} ${profile.surname}` : 'Courier'}
        </h1>
        {profile && <p className="text-brand-cyan text-xs mt-1">{profile.code}</p>}
      </div>

      {/* Summary Cards */}
      <div className="grid grid-cols-2 gap-3">
        <button onClick={() => navigate('/portal/runs')} className="bg-white rounded-xl p-4 border border-border text-left hover:border-brand-cyan transition-colors">
          <div className="text-2xl font-bold text-brand-dark">{upcomingRuns}</div>
          <div className="text-xs text-text-muted mt-1">Upcoming Runs</div>
        </button>
        <button onClick={() => navigate('/portal/invoicing')} className="bg-white rounded-xl p-4 border border-border text-left hover:border-brand-cyan transition-colors">
          <div className="text-2xl font-bold text-brand-dark">
            ${uninvoiced ? uninvoiced.courier.total.toFixed(0) : '—'}
          </div>
          <div className="text-xs text-text-muted mt-1">Uninvoiced</div>
        </button>
        <button onClick={() => navigate('/portal/schedule')} className="bg-white rounded-xl p-4 border border-border text-left hover:border-brand-cyan transition-colors">
          <div className="text-2xl font-bold text-brand-dark">{pendingSchedules}</div>
          <div className="text-xs text-text-muted mt-1">Pending Schedules</div>
        </button>
        <button onClick={() => navigate('/portal/reports')} className="bg-white rounded-xl p-4 border border-border text-left hover:border-brand-cyan transition-colors">
          <div className="text-2xl font-bold text-brand-dark">{runs.length}</div>
          <div className="text-xs text-text-muted mt-1">Total Runs</div>
        </button>
      </div>

      {/* Quick Actions */}
      <div className="space-y-2">
        <h2 className="text-sm font-semibold text-text-primary">Quick Actions</h2>
        <div className="flex gap-2 overflow-x-auto pb-1">
          {[
            { label: 'View Schedule', icon: '📅', to: '/portal/schedule' },
            { label: 'My Runs', icon: '🚚', to: '/portal/runs' },
            { label: 'Create Invoice', icon: '💰', to: '/portal/invoicing' },
          ].map(a => (
            <button
              key={a.to}
              onClick={() => navigate(a.to)}
              className="flex-shrink-0 bg-white border border-border rounded-lg px-4 py-3 flex items-center gap-2 text-sm hover:border-brand-cyan transition-colors"
            >
              <span>{a.icon}</span> {a.label}
            </button>
          ))}
        </div>
      </div>

      {/* Recent Activity */}
      <div>
        <h2 className="text-sm font-semibold text-text-primary mb-2">Recent Activity</h2>
        {runs.length === 0 ? (
          <div className="text-center py-6 text-text-muted text-sm">No recent runs</div>
        ) : (
          <div className="space-y-2">
            {runs.slice(0, 3).map(run => (
              <button
                key={run.id}
                onClick={() => navigate('/portal/runs')}
                className="w-full bg-white rounded-lg border border-border p-3 flex items-center gap-3 text-left hover:border-brand-cyan transition-colors"
              >
                <div className={`w-2 h-2 rounded-full flex-shrink-0 ${
                  run.status === 'In Progress' ? 'bg-brand-cyan' :
                  run.status === 'Completed' ? 'bg-success' : 'bg-warning'
                }`} />
                <div className="flex-1 min-w-0">
                  <div className="text-sm font-medium truncate">{run.name}</div>
                  <div className="text-xs text-text-muted">{run.location} · {run.startTime}–{run.endTime}</div>
                </div>
                <div className="text-xs text-text-muted flex-shrink-0">{run.status}</div>
              </button>
            ))}
          </div>
        )}
      </div>
    </div>
  );
}
