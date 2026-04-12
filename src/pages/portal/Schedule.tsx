import { useState, useCallback, useEffect } from 'react';
import { portalCourierService } from '@/services/portal_courierService';
import type { Schedule, TimeSlot } from '@/services/portal_mockData';

type Tab = 'pending' | 'available' | 'unavailable' | 'cancelled';
const TABS: { key: Tab; label: string; statusId: number | null }[] = [
  { key: 'pending', label: 'Pending', statusId: null },
  { key: 'available', label: 'Available', statusId: 1 },
  { key: 'unavailable', label: 'Unavailable', statusId: 2 },
  { key: 'cancelled', label: 'Cancelled', statusId: 3 },
];

function formatTime(_date: string, time: string) {
  const [h, m] = time.split(':');
  const hr = parseInt(h);
  return `${hr > 12 ? hr - 12 : hr || 12}:${m} ${hr >= 12 ? 'PM' : 'AM'}`;
}

function formatDate(iso: string) {
  const d = new Date(iso);
  return d.toLocaleDateString('en-NZ', { weekday: 'short', month: 'short', day: 'numeric' });
}

function formatSlotTime(iso: string) {
  return new Date(iso).toLocaleTimeString('en-NZ', { hour: 'numeric', minute: '2-digit' });
}

export default function PortalSchedule() {
  const [tab, setTab] = useState<Tab>('pending');
  const [schedules, setSchedules] = useState<Schedule[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [swipedId, setSwipedId] = useState<number | null>(null);
  const [timeSlotsFor, setTimeSlotsFor] = useState<Schedule | null>(null);

  useEffect(() => {
    let cancelled = false;
    async function load() {
      setLoading(true);
      setError(null);
      try {
        const data = await portalCourierService.getSchedules();
        if (!cancelled) setSchedules(data);
      } catch (e) {
        if (!cancelled) setError('Failed to load schedules.');
        console.error('Schedule load failed:', e);
      } finally {
        if (!cancelled) setLoading(false);
      }
    }
    load();
    return () => { cancelled = true; };
  }, []);

  const filtered = schedules.filter(s => {
    if (tab === 'pending') return !s.response;
    return s.response?.statusId === TABS.find(t => t.key === tab)!.statusId;
  });

  const markAvailable = useCallback(async (id: number, timeSlotId?: number) => {
    // Optimistic update
    setSchedules(prev => prev.map(s =>
      s.id === id ? { ...s, response: { statusId: 1, timeSlot: timeSlotId ? s.timeSlots.find(t => t.id === timeSlotId) : undefined } } : s
    ));
    setSwipedId(null);
    setTimeSlotsFor(null);
    try {
      await portalCourierService.markAvailable(id, timeSlotId);
    } catch (e) {
      console.error('markAvailable failed:', e);
      // Revert on failure
      setSchedules(prev => prev.map(s =>
        s.id === id ? { ...s, response: null } : s
      ));
    }
  }, []);

  const markUnavailable = useCallback(async (id: number) => {
    // Optimistic update
    setSchedules(prev => prev.map(s =>
      s.id === id ? { ...s, response: { statusId: 2 } } : s
    ));
    setSwipedId(null);
    try {
      await portalCourierService.markUnavailable(id);
    } catch (e) {
      console.error('markUnavailable failed:', e);
      // Revert on failure
      setSchedules(prev => prev.map(s =>
        s.id === id ? { ...s, response: null } : s
      ));
    }
  }, []);

  const allSlotsFull = (s: Schedule) => s.timeSlots.length > 0 && s.timeSlots.every(t => t.wanted && t.remaining === 0);

  if (loading) {
    return (
      <div className="flex items-center justify-center py-16">
        <div className="text-text-muted text-sm">Loading schedules…</div>
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
    <div className="space-y-3">
      {/* Tabs */}
      <div className="flex bg-white rounded-lg border border-border overflow-hidden">
        {TABS.map(t => (
          <button
            key={t.key}
            onClick={() => setTab(t.key)}
            className={`flex-1 py-2.5 text-xs font-medium transition-colors ${
              tab === t.key ? 'bg-brand-cyan text-white' : 'text-text-muted hover:bg-surface-light'
            }`}
          >
            {t.label}
          </button>
        ))}
      </div>

      {/* Schedule List */}
      {filtered.length === 0 && (
        <div className="text-center py-12 text-text-muted text-sm">No schedules in this tab</div>
      )}

      <div className="space-y-2">
        {filtered.map(schedule => (
          <div key={schedule.id} className="relative overflow-hidden rounded-xl">
            {/* Swipe actions (pending & unavailable tabs) */}
            {(tab === 'pending' || tab === 'unavailable') && (
              <div className="absolute inset-0 flex">
                <button
                  onClick={() => {
                    if (schedule.timeSlots.length > 0) {
                      setTimeSlotsFor(schedule);
                    } else {
                      markAvailable(schedule.id);
                    }
                  }}
                  className="flex-1 bg-success flex items-center justify-center text-white font-bold text-sm"
                >
                  AVAILABLE
                </button>
                {tab === 'pending' && (
                  <button
                    onClick={() => markUnavailable(schedule.id)}
                    className="flex-1 bg-error flex items-center justify-center text-white font-bold text-sm"
                  >
                    UNAVAILABLE
                  </button>
                )}
              </div>
            )}

            {/* Card */}
            <div
              onClick={() => {
                if (tab === 'pending' || tab === 'unavailable') {
                  setSwipedId(swipedId === schedule.id ? null : schedule.id);
                }
              }}
              className={`relative bg-white border border-border rounded-xl p-4 transition-transform duration-200 ${
                swipedId === schedule.id ? 'translate-x-[-40%]' : ''
              } ${
                tab === 'available' ? 'border-l-4 border-l-success' :
                tab === 'unavailable' ? 'border-l-4 border-l-error' :
                tab === 'cancelled' ? 'border-l-4 border-l-text-muted opacity-60' : ''
              }`}
            >
              <div className="flex items-center gap-3">
                <div className="flex-shrink-0 text-center">
                  <div className="text-xs font-semibold text-brand-cyan">{formatDate(schedule.bookDate)}</div>
                </div>
                <div className="flex-1 min-w-0">
                  <div className="font-medium text-sm truncate">{schedule.name}</div>
                  <div className="text-xs text-text-muted">{schedule.location}</div>
                  <div className="text-xs text-text-muted mt-0.5">
                    {formatTime(schedule.bookDate, schedule.startTime)} – {formatTime(schedule.bookDate, schedule.endTime)}
                  </div>
                </div>
                <div className="text-right flex-shrink-0">
                  {tab === 'available' && schedule.response?.timeSlot && (
                    <div className="text-xs font-medium text-success">{formatSlotTime(schedule.response.timeSlot.bookDateTime)}</div>
                  )}
                  {tab === 'available' && !schedule.response?.timeSlot && schedule.hasTimeSlots && (
                    <div className="text-xs font-medium text-warning">RESERVE</div>
                  )}
                  <div className="text-lg font-bold text-brand-dark">{schedule.wanted}</div>
                  <div className="text-[10px] text-text-muted">Wanted</div>
                </div>
              </div>
            </div>
          </div>
        ))}
      </div>

      {/* Time Slots Modal */}
      {timeSlotsFor && (
        <>
          <div className="fixed inset-0 bg-black/50 z-50" onClick={() => setTimeSlotsFor(null)} />
          <div className="fixed bottom-0 left-0 right-0 bg-white rounded-t-2xl z-50 p-4 max-h-[70vh] overflow-y-auto animate-slideUp">
            <h3 className="font-bold text-lg mb-1">Time Slots</h3>
            <p className="text-xs text-text-muted mb-4">Choose your initial pickup window</p>
            <div className="space-y-2">
              {timeSlotsFor.timeSlots.map((slot: TimeSlot) => (
                <button
                  key={slot.id}
                  disabled={slot.remaining === 0}
                  onClick={() => slot.remaining !== 0 && markAvailable(timeSlotsFor.id, slot.id)}
                  className={`w-full flex items-center justify-between p-3 rounded-lg border transition-colors ${
                    slot.remaining === 0
                      ? 'border-border bg-surface-light opacity-50'
                      : 'border-border hover:border-brand-cyan'
                  }`}
                >
                  <span className="font-medium text-sm">{formatSlotTime(slot.bookDateTime)}</span>
                  <span className={`text-xs ${slot.remaining === 0 ? 'text-error font-bold' : 'text-text-muted'}`}>
                    {slot.remaining === 0 ? 'FULL' : `${slot.remaining ?? 'N/A'} spots`}
                  </span>
                </button>
              ))}
              {allSlotsFull(timeSlotsFor) && (
                <button
                  onClick={() => markAvailable(timeSlotsFor.id)}
                  className="w-full p-3 rounded-lg border border-warning bg-warning/10 text-center"
                >
                  <div className="font-bold text-sm text-warning">RESERVE</div>
                  <div className="text-xs text-text-muted">Reserve for when a slot opens</div>
                </button>
              )}
            </div>
            <button onClick={() => setTimeSlotsFor(null)} className="w-full mt-4 py-3 text-sm text-text-muted">
              Cancel
            </button>
          </div>
        </>
      )}
    </div>
  );
}
