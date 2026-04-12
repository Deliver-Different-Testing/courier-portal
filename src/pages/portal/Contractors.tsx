import { useState, useEffect } from 'react';
import { portalCourierService, type Subcontractor } from '@/services/portal_courierService';

export default function PortalContractors() {
  const [subcontractors, setSubcontractors] = useState<Subcontractor[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [selected, setSelected] = useState<Subcontractor | null>(null);

  useEffect(() => {
    let cancelled = false;
    async function load() {
      setLoading(true);
      setError(null);
      try {
        const data = await portalCourierService.getSubcontractors();
        if (!cancelled) setSubcontractors(data);
      } catch (e) {
        if (!cancelled) setError('Failed to load subcontractors.');
        console.error('Contractors load failed:', e);
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

  if (selected) {
    return (
      <div className="space-y-4">
        <button onClick={() => setSelected(null)} className="text-sm text-brand-cyan">← Back</button>
        <div className="bg-white rounded-xl border border-border p-4">
          <div className="flex items-center gap-3 mb-4">
            <div className="w-12 h-12 rounded-full bg-brand-cyan/20 flex items-center justify-center text-lg font-bold text-brand-cyan">
              {selected.name.split(' ').map(n => n[0]).join('')}
            </div>
            <div>
              <div className="font-bold">{selected.name}</div>
              <div className="text-xs text-text-muted">{selected.code} · {selected.phone}</div>
            </div>
            <span className={`ml-auto px-2 py-0.5 rounded-full text-[10px] font-medium ${
              selected.status === 'Active' ? 'bg-success/20 text-success' : 'bg-error/20 text-error'
            }`}>{selected.status}</span>
          </div>
          <div className="grid grid-cols-2 gap-3">
            <div className="bg-surface-light rounded-lg p-3 text-center">
              <div className="text-xl font-bold">{selected.runsCompleted}</div>
              <div className="text-[10px] text-text-muted">Runs Completed</div>
            </div>
            <div className="bg-surface-light rounded-lg p-3 text-center">
              <div className="text-xl font-bold">${selected.totalEarnings.toLocaleString()}</div>
              <div className="text-[10px] text-text-muted">Total Earnings</div>
            </div>
          </div>
        </div>
      </div>
    );
  }

  return (
    <div className="space-y-3">
      <h2 className="font-semibold text-sm">Subcontractors</h2>
      {subcontractors.length === 0 && (
        <div className="text-center py-12 text-text-muted text-sm">No subcontractors found</div>
      )}
      {subcontractors.map(sc => (
        <button
          key={sc.id}
          onClick={() => setSelected(sc)}
          className="w-full bg-white rounded-xl border border-border p-3 flex items-center gap-3 text-left hover:border-brand-cyan transition-colors"
        >
          <div className="w-10 h-10 rounded-full bg-brand-cyan/20 flex items-center justify-center text-sm font-bold text-brand-cyan">
            {sc.name.split(' ').map(n => n[0]).join('')}
          </div>
          <div className="flex-1 min-w-0">
            <div className="text-sm font-medium truncate">{sc.name}</div>
            <div className="text-xs text-text-muted">{sc.code} · {sc.runsCompleted} runs</div>
          </div>
          <span className={`px-2 py-0.5 rounded-full text-[10px] font-medium ${
            sc.status === 'Active' ? 'bg-success/20 text-success' : 'bg-error/20 text-error'
          }`}>{sc.status}</span>
        </button>
      ))}
    </div>
  );
}
