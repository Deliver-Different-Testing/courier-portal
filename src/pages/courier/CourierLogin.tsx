import { useState, FormEvent } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import { resolveTenant } from '@/lib/tenants';

export default function CourierLogin() {
  const { tenantSlug } = useParams<{ tenantSlug: string }>();
  const tenant = resolveTenant(tenantSlug);
  const navigate = useNavigate();

  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('');
  const [remember, setRemember] = useState(false);
  const [error, setError] = useState('');
  const [loading, setLoading] = useState(false);

  const handleSubmit = async (e: FormEvent) => {
    e.preventDefault();
    setError('');
    if (!email || !password) { setError('Email and password are required.'); return; }
    setLoading(true);
    // TODO: wire to real API — POST /api/auth/login
    setTimeout(() => {
      // Mock: store token and navigate
      sessionStorage.setItem('courierToken', 'mock-token');
      sessionStorage.setItem('courierTenant', tenant.slug);
      navigate(`/courier/${tenant.slug}/dashboard`);
    }, 800);
  };

  return (
    <div className="min-h-screen flex flex-col" style={{ background: tenant.primaryColor }}>
      {/* Header */}
      <div className="flex flex-col items-center pt-16 pb-8 px-6">
        {tenant.logoUrl ? (
          <img src={tenant.logoUrl} alt={tenant.name} className="h-16 w-auto mb-4 rounded-lg" />
        ) : (
          <div className="h-16 w-16 rounded-2xl flex items-center justify-center text-2xl font-bold mb-4"
               style={{ background: tenant.accentColor, color: tenant.primaryColor }}>
            {tenant.name.charAt(0)}
          </div>
        )}
        <h1 className="text-white text-xl font-bold">{tenant.name}</h1>
        <p className="text-white/60 text-sm mt-1">Courier Portal</p>
      </div>

      {/* Login Card */}
      <div className="flex-1 bg-white rounded-t-3xl px-6 pt-8 pb-6">
        <h2 className="text-xl font-bold text-gray-900 mb-6">Sign In</h2>

        {error && (
          <div className="bg-red-50 text-red-700 text-sm px-4 py-3 rounded-xl mb-4">{error}</div>
        )}

        <form onSubmit={handleSubmit} className="space-y-4">
          <div>
            <label className="block text-sm font-medium text-gray-700 mb-1">Email</label>
            <input
              type="email" autoComplete="email" value={email}
              onChange={e => setEmail(e.target.value)}
              className="w-full h-12 px-4 rounded-xl border border-gray-300 text-base focus:ring-2 focus:border-transparent"
              style={{ '--tw-ring-color': tenant.accentColor } as React.CSSProperties}
              placeholder="you@example.com"
            />
          </div>
          <div>
            <label className="block text-sm font-medium text-gray-700 mb-1">Password</label>
            <input
              type="password" autoComplete="current-password" value={password}
              onChange={e => setPassword(e.target.value)}
              className="w-full h-12 px-4 rounded-xl border border-gray-300 text-base focus:ring-2 focus:border-transparent"
              style={{ '--tw-ring-color': tenant.accentColor } as React.CSSProperties}
              placeholder="••••••••"
            />
          </div>

          <div className="flex items-center justify-between">
            <label className="flex items-center gap-2 text-sm text-gray-600">
              <input type="checkbox" checked={remember} onChange={e => setRemember(e.target.checked)}
                     className="rounded" style={{ accentColor: tenant.accentColor }} />
              Remember me
            </label>
            <button type="button" className="text-sm font-medium" style={{ color: tenant.accentColor }}>
              Forgot password?
            </button>
          </div>

          <button
            type="submit" disabled={loading}
            className="w-full h-12 rounded-xl text-base font-semibold text-white disabled:opacity-50 transition-all active:scale-[0.98]"
            style={{ background: tenant.accentColor }}
          >
            {loading ? 'Signing in…' : 'Sign In'}
          </button>
        </form>

        <div className="mt-8 text-center">
          <p className="text-sm text-gray-500">
            Want to become a courier?{' '}
            <a href={`#/apply/${tenant.slug}`} className="font-medium" style={{ color: tenant.accentColor }}>
              Apply here
            </a>
          </p>
        </div>
      </div>
    </div>
  );
}
