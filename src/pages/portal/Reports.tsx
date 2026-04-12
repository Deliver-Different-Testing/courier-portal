import { useState, useEffect } from 'react';
import { portalCourierService, type ReportSummary } from '@/services/portal_courierService';

export default function PortalReports() {
  const [report, setReport] = useState<ReportSummary | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    let cancelled = false;
    async function load() {
      setLoading(true);
      setError(null);
      try {
        const data = await portalCourierService.getReportSummary();
        if (!cancelled) setReport(data);
      } catch (e) {
        if (!cancelled) setError('Failed to load report data.');
        console.error('Reports load failed:', e);
      } finally {
        if (!cancelled) setLoading(false);
      }
    }
    load();
    return () => { cancelled = true; };
  }, []);

  if (loading) {
    return (
      <div className="flex items-center justify-center py-16">
        <div className="text-text-muted text-sm">Loading reports…</div>
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

  if (!report) return null;

  const r = report;
  const maxEarnings = r.weeklyData.length > 0 ? Math.max(...r.weeklyData.map(w => w.earnings)) : 1;

  return (
    <div className="space-y-4">
      {/* Summary Cards */}
      <div className="grid grid-cols-2 gap-3">
        <div className="bg-white rounded-xl border border-border p-4 text-center">
          <div className="text-2xl font-bold text-brand-dark">{r.totalRuns}</div>
          <div className="text-xs text-text-muted">Total Runs</div>
        </div>
        <div className="bg-white rounded-xl border border-border p-4 text-center">
          <div className="text-2xl font-bold text-brand-dark">${r.totalEarnings.toLocaleString()}</div>
          <div className="text-xs text-text-muted">Total Earnings</div>
        </div>
        <div className="bg-white rounded-xl border border-border p-4 text-center">
          <div className="text-2xl font-bold text-brand-dark">{r.thisWeekRuns}</div>
          <div className="text-xs text-text-muted">This Week</div>
        </div>
        <div className="bg-white rounded-xl border border-border p-4 text-center">
          <div className="text-2xl font-bold text-brand-dark">${r.thisWeekEarnings.toLocaleString()}</div>
          <div className="text-xs text-text-muted">Week Earnings</div>
        </div>
      </div>

      {/* This Month */}
      <div className="bg-brand-dark rounded-xl p-4 text-white">
        <div className="text-xs text-white/60">This Month</div>
        <div className="flex items-baseline gap-4 mt-1">
          <div><span className="text-2xl font-bold">{r.thisMonthRuns}</span> <span className="text-xs text-white/60">runs</span></div>
          <div><span className="text-2xl font-bold">${r.thisMonthEarnings.toLocaleString()}</span> <span className="text-xs text-white/60">earned</span></div>
        </div>
        <div className="text-xs text-white/40 mt-1">Avg ${r.avgPerRun}/run</div>
      </div>

      {/* Weekly Bar Chart */}
      {r.weeklyData.length > 0 && (
        <div className="bg-white rounded-xl border border-border p-4">
          <h3 className="font-semibold text-sm mb-3">Weekly Earnings</h3>
          <div className="flex items-end gap-2 h-32">
            {r.weeklyData.map(w => (
              <div key={w.week} className="flex-1 flex flex-col items-center gap-1">
                <div className="text-[10px] text-text-muted font-medium">${w.earnings}</div>
                <div
                  className="w-full bg-brand-cyan rounded-t"
                  style={{ height: `${(w.earnings / maxEarnings) * 100}%`, minHeight: 4 }}
                />
                <div className="text-[10px] text-text-muted">{w.week.split(' ')[0]}</div>
              </div>
            ))}
          </div>
        </div>
      )}
    </div>
  );
}
