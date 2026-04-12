import { useState } from 'react';
import { useParams } from 'react-router-dom';
import { resolveTenant } from '@/lib/tenants';

interface TimeSlot {
  id: number;
  bookDateTime: string;
  remaining: number | null; // null = N/A, 0 = FULL
}

interface Schedule {
  id: number;
  bookDate: string;
  name: string;
  location: string;
  startTime: string;
  endTime: string;
  wanted: number;
  hasTimeSlots: boolean;
  timeSlots: TimeSlot[];
  response: { statusId: number; timeSlot?: TimeSlot } | null;
  // statusId: 1=available, 2=unavailable, 3=cancelled
}

const MOCK_SCHEDULES: Schedule[] = [
  {
    id: 1, bookDate: '2026-03-09', name: 'Auckland AM', location: 'Auckland CBD',
    startTime: '06:00', endTime: '12:00', wanted: 5, hasTimeSlots: true,
    timeSlots: [
      { id: 1, bookDateTime: '2026-03-09T06:00', remaining: 3 },
      { id: 2, bookDateTime: '2026-03-09T07:00', remaining: 1 },
      { id: 3, bookDateTime: '2026-03-09T08:00', remaining: 0 },
    ],
    response: null,
  },
  {
    id: 2, bookDate: '2026-03-09', name: 'Auckland PM', location: 'South Auckland',
    startTime: '13:00', endTime: '19:00', wanted: 3, hasTimeSlots: false, timeSlots: [],
    response: null,
  },
  {
    id: 3, bookDate: '2026-03-10', name: 'Auckland AM', location: 'Auckland CBD',
    startTime: '06:00', endTime: '12:00', wanted: 4, hasTimeSlots: true,
    timeSlots: [{ id: 4, bookDateTime: '2026-03-10T07:00', remaining: 2 }],
    response: { statusId: 1, timeSlot: { id: 4, bookDateTime: '2026-03-10T07:00', remaining: 2 } },
  },
  {
    id: 4, bookDate: '2026-03-11', name: 'Hamilton Run', location: 'Hamilton',
    startTime: '07:00', endTime: '15:00', wanted: 2, hasTimeSlots: false, timeSlots: [],
    response: { statusId: 2 },
  },
];

type Tab = 'pending' | 'available' | 'unavailable' | 'cancelled';

const TABS: { key: Tab; label: string }[] = [
  { key: 'pending', label: 'Pending' },
  { key: 'available', label: 'Available' },
  { key: 'unavailable', label: 'Unavailable' },
  { key: 'cancelled', label: 'Cancelled' },
];

function formatTime(t: string) {
  const [h, m] = t.split(':').map(Number);
  const ampm = h >= 12 ? 'PM' : 'AM';
  const h12 = h % 12 || 12;
  return `${h12}:${String(m).padStart(2, '0')} ${ampm}`;
}

function dayOfWeek(d: string) {
  return new Date(d + 'T00:00').toLocaleDateString('en-US', { weekday: 'long' });
}

function formatDate(d: string) {
  return new Date(d + 'T00:00').toLocaleDateString('en-NZ', { day: 'numeric', month: 'short', year: 'numeric' });
}

export default function CourierSchedule() {
  const { tenantSlug } = useParams<{ tenantSlug: string }>();
  const tenant = resolveTenant(tenantSlug);
  const [tab, setTab] = useState<Tab>('pending');
  const [schedules, setSchedules] = useState(MOCK_SCHEDULES);
  const [showTimeSlots, setShowTimeSlots] = useState<number | null>(null);

  const filtered = schedules.filter(s => {
    if (tab === 'pending') return !s.response;
    if (tab === 'available') return s.response?.statusId === 1;
    if (tab === 'unavailable') return s.response?.statusId === 2;
    if (tab === 'cancelled') return s.response?.statusId === 3;
    return false;
  });

  const handleAvailable = (sched: Schedule, timeSlot?: TimeSlot) => {
    setSchedules(prev => prev.map(s =>
      s.id === sched.id ? { ...s, response: { statusId: 1, timeSlot } } : s
    ));
    setShowTimeSlots(null);
  };

  const handleUnavailable = (sched: Schedule) => {
    setSchedules(prev => prev.map(s =>
      s.id === sched.id ? { ...s, response: { statusId: 2 } } : s
    ));
  };

  const selectedSched = schedules.find(s => s.id === showTimeSlots);

  return (
    <div className="px-4 pt-4">
      <h1 className="text-xl font-bold text-gray-900 mb-4">Schedule</h1>

      {/* Tabs */}
      <div className="flex bg-gray-100 rounded-xl p-1 mb-4 overflow-x-auto">
        {TABS.map(t => (
          <button key={t.key} onClick={() => setTab(t.key)}
            className={`flex-1 py-2 text-xs font-semibold rounded-lg transition-all whitespace-nowrap px-2 ${
              tab === t.key ? 'bg-white shadow-sm text-gray-900' : 'text-gray-500'
            }`}
            style={tab === t.key ? { color: tenant.accentColor } : undefined}
          >
            {t.label}
          </button>
        ))}
      </div>

      {/* Schedule Cards */}
      {filtered.length === 0 && (
        <div className="text-center text-gray-400 py-12">
          <div className="text-4xl mb-2">📅</div>
          No {tab} schedules
        </div>
      )}

      {filtered.map(sched => (
        <div key={sched.id} className="bg-white rounded-xl p-4 shadow-sm border border-gray-100 mb-3">
          <div className="flex justify-between items-start">
            <div className="flex-1 min-w-0">
              <div className="font-semibold text-gray-900 text-sm">{dayOfWeek(sched.bookDate)}</div>
              <div className="text-xs text-gray-400">{formatDate(sched.bookDate)}</div>
              <div className="text-sm text-gray-700 mt-1 font-medium">{sched.name}</div>
              <div className="text-xs text-gray-500">{sched.location}</div>
              <div className="text-xs text-gray-400 mt-0.5">
                {formatTime(sched.startTime)} – {formatTime(sched.endTime)}
              </div>
            </div>
            <div className="text-right shrink-0">
              {tab === 'available' && sched.response?.timeSlot && (
                <>
                  <div className="font-bold text-sm" style={{ color: tenant.accentColor }}>
                    {new Date(sched.response.timeSlot.bookDateTime).toLocaleTimeString('en-NZ', { hour: 'numeric', minute: '2-digit' })}
                  </div>
                  <div className="text-xs text-gray-400">Time Slot</div>
                </>
              )}
              {tab === 'available' && !sched.response?.timeSlot && sched.hasTimeSlots && (
                <span className="text-xs font-bold px-2 py-1 rounded-full bg-amber-50 text-amber-600">RESERVE</span>
              )}
              {(tab === 'pending' || tab === 'unavailable' || tab === 'cancelled') && (
                <>
                  <div className="font-bold text-lg" style={{ color: tenant.accentColor }}>{sched.wanted}</div>
                  <div className="text-xs text-gray-400">Wanted</div>
                </>
              )}
            </div>
          </div>

          {/* Action buttons for pending/unavailable */}
          {tab === 'pending' && (
            <div className="flex gap-2 mt-3">
              <button
                onClick={() => sched.hasTimeSlots ? setShowTimeSlots(sched.id) : handleAvailable(sched)}
                className="flex-1 py-2.5 rounded-xl text-sm font-semibold text-white active:scale-[0.97] transition-transform"
                style={{ background: '#10b981' }}
              >
                Available
              </button>
              <button
                onClick={() => handleUnavailable(sched)}
                className="flex-1 py-2.5 rounded-xl text-sm font-semibold text-white bg-red-500 active:scale-[0.97] transition-transform"
              >
                Unavailable
              </button>
            </div>
          )}
          {tab === 'unavailable' && (
            <button
              onClick={() => sched.hasTimeSlots ? setShowTimeSlots(sched.id) : handleAvailable(sched)}
              className="w-full mt-3 py-2.5 rounded-xl text-sm font-semibold text-white active:scale-[0.97] transition-transform"
              style={{ background: '#10b981' }}
            >
              Mark Available
            </button>
          )}
        </div>
      ))}

      {/* Time Slots Modal */}
      {showTimeSlots != null && selectedSched && (
        <div className="fixed inset-0 bg-black/50 z-50 flex items-end justify-center" onClick={() => setShowTimeSlots(null)}>
          <div className="bg-white w-full max-w-lg rounded-t-2xl p-6 pb-8 safe-area-bottom" onClick={e => e.stopPropagation()}>
            <h3 className="font-bold text-lg text-gray-900 mb-1">Time Slots</h3>
            <p className="text-sm text-gray-500 mb-4">Choose your initial pickup window.</p>
            <div className="space-y-2">
              {selectedSched.timeSlots.map(ts => {
                const full = ts.remaining === 0;
                return (
                  <button key={ts.id} disabled={full}
                    onClick={() => !full && handleAvailable(selectedSched, ts)}
                    className={`w-full flex justify-between items-center p-4 rounded-xl border text-left ${
                      full ? 'bg-gray-50 border-gray-200 opacity-50' : 'border-gray-200 active:bg-gray-50'
                    }`}
                  >
                    <span className="font-semibold text-sm">
                      {new Date(ts.bookDateTime).toLocaleTimeString('en-NZ', { hour: 'numeric', minute: '2-digit' })}
                    </span>
                    <span className={`text-xs font-medium ${full ? 'text-red-500' : 'text-green-600'}`}>
                      {full ? 'FULL' : ts.remaining == null ? 'N/A' : `${ts.remaining} spots remaining`}
                    </span>
                  </button>
                );
              })}
              {selectedSched.timeSlots.every(ts => ts.remaining === 0) && (
                <button
                  onClick={() => handleAvailable(selectedSched)}
                  className="w-full p-4 rounded-xl border border-amber-300 bg-amber-50 text-left active:bg-amber-100"
                >
                  <div className="font-semibold text-sm text-amber-700">RESERVE</div>
                  <div className="text-xs text-amber-600 mt-0.5">Reserve a spot for when a time slot becomes available.</div>
                </button>
              )}
            </div>
            <button onClick={() => setShowTimeSlots(null)}
              className="w-full mt-4 py-3 rounded-xl text-sm font-semibold text-gray-500 bg-gray-100 active:bg-gray-200">
              Cancel
            </button>
          </div>
        </div>
      )}
    </div>
  );
}
