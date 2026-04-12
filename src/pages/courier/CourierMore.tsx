import { useParams, useNavigate } from 'react-router-dom';
import { resolveTenant } from '@/lib/tenants';

export default function CourierMore() {
  const { tenantSlug } = useParams<{ tenantSlug: string }>();
  const tenant = resolveTenant(tenantSlug);
  const navigate = useNavigate();

  const items = [
    { label: 'Invoicing', icon: '📄', path: 'invoicing' },
    { label: 'Sub-Contractors', icon: '👥', path: 'contractors' },
    { label: 'Settings', icon: '⚙️', path: 'settings' },
  ];

  return (
    <div className="px-4 pt-4">
      <h1 className="text-xl font-bold text-gray-900 mb-4">More</h1>
      {items.map(item => (
        <button key={item.path}
          onClick={() => navigate(`/courier/${tenant.slug}/${item.path}`)}
          className="w-full flex items-center gap-3 bg-white rounded-xl p-4 shadow-sm border border-gray-100 mb-3 text-left active:bg-gray-50 transition-colors">
          <span className="text-xl">{item.icon}</span>
          <span className="font-medium text-gray-900 text-sm">{item.label}</span>
          <span className="ml-auto text-gray-400">›</span>
        </button>
      ))}

      <button
        onClick={() => { sessionStorage.clear(); navigate(`/courier/${tenant.slug}`); }}
        className="w-full mt-6 py-3 rounded-xl text-sm font-semibold text-red-600 bg-red-50 active:bg-red-100">
        Sign Out
      </button>
    </div>
  );
}
