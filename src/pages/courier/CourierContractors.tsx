import { useState } from 'react';
import { useParams } from 'react-router-dom';
import { resolveTenant } from '@/lib/tenants';

interface Contractor {
  id: number;
  name: string;
  type: number; // 2=master, 3=sub
  percentage: number;
  fuelPercentage: number;
}

const MOCK_CONTRACTORS: Contractor[] = [
  { id: 1, name: 'John Smith', type: 3, percentage: 0.75, fuelPercentage: 0.80 },
  { id: 2, name: 'Sarah Jones', type: 3, percentage: 0.70, fuelPercentage: 0.75 },
];

export default function CourierContractors() {
  const { tenantSlug } = useParams<{ tenantSlug: string }>();
  const tenant = resolveTenant(tenantSlug);
  const [contractors, setContractors] = useState(MOCK_CONTRACTORS);
  const [editing, setEditing] = useState<number | null>(null);
  const [editPct, setEditPct] = useState(0);
  const [editFuelPct, setEditFuelPct] = useState(0);

  const startEdit = (c: Contractor) => {
    setEditing(c.id);
    setEditPct(c.percentage);
    setEditFuelPct(c.fuelPercentage);
  };

  const saveEdit = () => {
    if (editing == null) return;
    setContractors(prev => prev.map(c =>
      c.id === editing ? { ...c, percentage: editPct, fuelPercentage: editFuelPct } : c
    ));
    setEditing(null);
  };

  return (
    <div className="px-4 pt-4">
      <h1 className="text-xl font-bold text-gray-900 mb-4">Sub-Contractors</h1>

      {contractors.length === 0 && (
        <div className="text-center text-gray-400 py-12">
          <div className="text-4xl mb-2">👥</div>
          No sub-contractors
        </div>
      )}

      {contractors.map(c => (
        <div key={c.id} className="bg-white rounded-xl p-4 shadow-sm border border-gray-100 mb-3">
          {editing === c.id ? (
            <>
              <div className="font-semibold text-gray-900 mb-3">{c.name}</div>
              <div className="space-y-3">
                <div>
                  <label className="text-xs font-medium text-gray-500">Percentage</label>
                  <input type="number" min={0} max={1} step={0.01} value={editPct}
                    onChange={e => setEditPct(parseFloat(e.target.value) || 0)}
                    className="w-full h-11 px-3 mt-1 rounded-xl border border-gray-300 text-sm" />
                </div>
                <div>
                  <label className="text-xs font-medium text-gray-500">Fuel Percentage</label>
                  <input type="number" min={0} max={1} step={0.01} value={editFuelPct}
                    onChange={e => setEditFuelPct(parseFloat(e.target.value) || 0)}
                    className="w-full h-11 px-3 mt-1 rounded-xl border border-gray-300 text-sm" />
                </div>
              </div>
              <div className="flex gap-2 mt-3">
                <button onClick={saveEdit}
                  className="flex-1 py-2.5 rounded-xl text-sm font-semibold text-white active:scale-[0.97]"
                  style={{ background: tenant.accentColor }}>
                  Save
                </button>
                <button onClick={() => setEditing(null)}
                  className="flex-1 py-2.5 rounded-xl text-sm font-semibold text-gray-600 bg-gray-100 active:scale-[0.97]">
                  Cancel
                </button>
              </div>
            </>
          ) : (
            <div className="flex items-center justify-between" onClick={() => startEdit(c)}>
              <div>
                <div className="font-semibold text-gray-900 text-sm">{c.name}</div>
                <div className="text-xs text-gray-500 mt-1">
                  Pay: {(c.percentage * 100).toFixed(0)}% · Fuel: {(c.fuelPercentage * 100).toFixed(0)}%
                </div>
              </div>
              <span className="text-gray-400 text-lg">›</span>
            </div>
          )}
        </div>
      ))}
    </div>
  );
}
