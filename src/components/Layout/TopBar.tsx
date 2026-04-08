import { useRole } from '@/context/RoleContext';
import { useMessengerUnread } from '@/components/common/CourierMessenger';

interface Props {
  onToggle: () => void;
  onMessengerToggle?: () => void;
}

export default function TopBar({ onToggle, onMessengerToggle }: Props) {
  const { role, logout } = useRole();
  const unreadCount = useMessengerUnread();

  const roleBadge = role === 'tenant'
    ? { label: 'Tenant', bg: 'bg-brand-cyan/10', text: 'text-brand-cyan' }
    : role === 'id'
    ? { label: 'In-House Driver', bg: 'bg-[#f59e0b]/10', text: 'text-[#f59e0b]' }
    : { label: 'Network Partner', bg: 'bg-brand-purple/10', text: 'text-brand-purple' };

  return (
    <div className="px-6 py-3 bg-white border-b border-border flex items-center gap-3">
      <button onClick={onToggle} className="bg-transparent border-none text-text-muted hover:text-text-primary text-xl cursor-pointer transition-colors">
        <svg className="w-5 h-5" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2">
          <line x1="3" y1="6" x2="21" y2="6" />
          <line x1="3" y1="12" x2="21" y2="12" />
          <line x1="3" y1="18" x2="21" y2="18" />
        </svg>
      </button>
      <div className="flex-1" />
      {onMessengerToggle && (
        <button
          onClick={onMessengerToggle}
          className="relative bg-transparent border-none text-text-muted hover:text-brand-cyan text-lg cursor-pointer transition-colors p-1"
          title="Messenger"
        >
          💬
          {unreadCount > 0 && (
            <span className="absolute -top-1 -right-1 bg-red-500 text-white text-[9px] font-bold rounded-full w-4 h-4 flex items-center justify-center">
              {unreadCount}
            </span>
          )}
        </button>
      )}
      <span className={`text-xs px-3 py-1 rounded-full font-medium ${roleBadge.bg} ${roleBadge.text}`}>
        {roleBadge.label}
      </span>
      <button
        onClick={logout}
        className="text-xs font-medium text-brand-cyan hover:text-brand-dark bg-brand-cyan/10 hover:bg-brand-cyan/20 transition-all px-3 py-1.5 rounded-lg border border-brand-cyan/20 hover:border-brand-cyan/40"
      >
        Switch Role
      </button>
    </div>
  );
}
