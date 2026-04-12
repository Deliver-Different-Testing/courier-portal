import { Outlet, useParams, useNavigate, useLocation } from 'react-router-dom';
import { resolveTenant } from '@/lib/tenants';

const NAV_ITEMS = [
  { key: 'dashboard', label: 'Home', icon: '📊', path: 'dashboard' },
  { key: 'runs', label: 'Runs', icon: '🚚', path: 'runs' },
  { key: 'documents', label: 'Docs', icon: '📄', path: 'documents' },
  { key: 'training', label: 'Training', icon: '🎓', path: 'training' },
  { key: 'more', label: 'More', icon: '☰', path: 'more' },
] as const;

export default function CourierLayout() {
  const { tenantSlug } = useParams<{ tenantSlug: string }>();
  const tenant = resolveTenant(tenantSlug);
  const navigate = useNavigate();
  const location = useLocation();

  const currentPath = location.pathname.split('/').pop() || 'dashboard';

  return (
    <div className="min-h-screen flex flex-col bg-gray-50">
      {/* Top Header */}
      <header className="safe-area-top flex items-center h-14 px-4 shrink-0"
              style={{ background: tenant.primaryColor }}>
        {tenant.logoUrl ? (
          <img src={tenant.logoUrl} alt={tenant.name} className="h-8 w-auto rounded" />
        ) : (
          <span className="text-white font-bold text-lg">{tenant.name}</span>
        )}
      </header>

      {/* Page Content */}
      <main className="flex-1 overflow-y-auto pb-20">
        <Outlet />
      </main>

      {/* Bottom Navigation */}
      <nav className="safe-area-bottom fixed bottom-0 inset-x-0 bg-white border-t border-gray-200 flex z-50">
        {NAV_ITEMS.map(item => {
          const active = currentPath === item.path || (item.key === 'more' && ['invoicing','contractors','settings','schedule'].includes(currentPath)) || (item.key === 'documents' && currentPath === 'documents');
          return (
            <button
              key={item.key}
              onClick={() => navigate(`/courier/${tenant.slug}/${item.path}`)}
              className={`flex-1 flex flex-col items-center py-2 text-xs transition-colors ${active ? 'font-semibold' : 'text-gray-400'}`}
              style={active ? { color: tenant.accentColor } : undefined}
            >
              <span className="text-xl mb-0.5">{item.icon}</span>
              {item.label}
            </button>
          );
        })}
      </nav>
    </div>
  );
}
