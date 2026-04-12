import { useState, useEffect } from 'react';
import StatCard from '@/components/common/StatCard';
import BarChart from '@/components/common/BarChart';
import { reportService, type ReportData } from '@/services/np_reportService';

export default function Reports() {
  const [from, setFrom] = useState('2026-02-22');
  const [to, setTo] = useState('2026-02-28');
  const [data, setData] = useState<ReportData>({ jobsCompleted: 0, onTimePercent: 0, revenue: '$0', dailyVolume: [] });

  useEffect(() => {
    reportService.getData(from, to).then(setData).catch(() => {});
  }, [from, to]);

  return (
    <>
      <h2 className="text-xl font-bold mb-5">Reports</h2>

      {/* Date Filter */}
      <div className="flex gap-3 mb-6 flex-wrap items-end">
        <div className="flex flex-col gap-1">
          <label className="text-xs text-text-secondary uppercase tracking-wide">From</label>
          <input type="date" value={from} onChange={e => setFrom(e.target.value)} className="w-40" />
        </div>
        <div className="flex flex-col gap-1">
          <label className="text-xs text-text-secondary uppercase tracking-wide">To</label>
          <input type="date" value={to} onChange={e => setTo(e.target.value)} className="w-40" />
        </div>
        <button className="bg-brand-cyan text-brand-dark border-none font-medium px-4 py-2 rounded-md text-sm hover:shadow-cyan-glow">
          Generate Report
        </button>
      </div>

      {/* Stat Cards */}
      <div className="grid grid-cols-1 sm:grid-cols-3 gap-4 mb-6">
        <StatCard label="Jobs Completed" value={data.jobsCompleted} color="green" />
        <StatCard label="On-Time %" value={`${data.onTimePercent}%`} color="cyan" />
        <StatCard label="Your Revenue" value={data.revenue} note="Rate-masked — customer pricing hidden" />
      </div>

      {/* Bar Chart */}
      <div className="bg-white border border-border rounded-lg p-5">
        <h3 className="font-bold mb-4">Daily Job Volume (Last 7 Days)</h3>
        <BarChart data={data.dailyVolume.map(d => ({ label: d.day, value: d.value }))} />
      </div>
    </>
  );
}
