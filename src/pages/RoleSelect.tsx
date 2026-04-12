import { useRole } from '@/context/RoleContext';

export default function RoleSelect() {
  const { setRole } = useRole();

  return (
    <div className="min-h-screen flex items-center justify-center bg-surface-light">
      <div className="max-w-4xl w-full px-6">
        {/* Logo & Title */}
        <div className="text-center mb-10">
          <div className="w-16 h-16 rounded-2xl bg-brand-cyan flex items-center justify-center mx-auto mb-4">
            <svg className="w-10 h-10 text-brand-dark" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2">
              <path d="M14 16H9m10 0h3v-3.15a1 1 0 0 0-.84-.99L16 11l-2.7-3.6a1 1 0 0 0-.8-.4H5.24a2 2 0 0 0-1.8 1.1l-.8 1.63A6 6 0 0 0 2 12.42V16h2" />
              <circle cx="6.5" cy="16.5" r="2.5" />
              <circle cx="16.5" cy="16.5" r="2.5" />
            </svg>
          </div>
          <h1 className="text-3xl font-bold text-text-primary">Fleet & Partners</h1>
          <p className="text-text-secondary mt-2 text-lg">Manage your delivery fleet and partners</p>
        </div>

        {/* Role Cards */}
        <div className="grid grid-cols-1 md:grid-cols-3 gap-6">
          <button
            onClick={() => setRole('tenant')}
            className="group bg-white border border-border rounded-lg p-8 text-left transition-all hover:border-brand-cyan hover:shadow-md hover:-translate-y-px"
          >
            <div className="w-12 h-12 rounded-lg bg-brand-cyan/10 flex items-center justify-center mb-4">
              <svg className="w-6 h-6 text-brand-cyan" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2">
                <path d="M3 9l9-7 9 7v11a2 2 0 0 1-2 2H5a2 2 0 0 1-2-2z" />
                <polyline points="9 22 9 12 15 12 15 22" />
              </svg>
            </div>
            <h2 className="text-xl font-bold text-text-primary mb-2">Tenant</h2>
            <p className="text-text-secondary text-sm leading-relaxed">
              Manage your agents, couriers, and delivery partners
            </p>
            <div className="mt-6 text-brand-cyan text-sm font-medium opacity-0 group-hover:opacity-100 transition-opacity">
              Enter →
            </div>
          </button>

          <button
            onClick={() => setRole('np')}
            className="group bg-white border border-border rounded-lg p-8 text-left transition-all hover:border-brand-purple hover:shadow-md hover:-translate-y-px"
          >
            <div className="w-12 h-12 rounded-lg bg-brand-purple/10 flex items-center justify-center mb-4">
              <svg className="w-6 h-6 text-brand-purple" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2">
                <path d="M14 16H9m10 0h3v-3.15a1 1 0 0 0-.84-.99L16 11l-2.7-3.6a1 1 0 0 0-.8-.4H5.24a2 2 0 0 0-1.8 1.1l-.8 1.63A6 6 0 0 0 2 12.42V16h2" />
                <circle cx="6.5" cy="16.5" r="2.5" />
                <circle cx="16.5" cy="16.5" r="2.5" />
              </svg>
            </div>
            <h2 className="text-xl font-bold text-text-primary mb-2">Network Partner</h2>
            <p className="text-text-secondary text-sm leading-relaxed">
              Manage your fleet and deliveries
            </p>
            <div className="mt-6 text-brand-purple text-sm font-medium opacity-0 group-hover:opacity-100 transition-opacity">
              Enter →
            </div>
          </button>

          <button
            onClick={() => setRole('id')}
            className="group bg-white border border-border rounded-lg p-8 text-left transition-all hover:border-[#f59e0b] hover:shadow-md hover:-translate-y-px"
          >
            <div className="w-12 h-12 rounded-lg bg-[#f59e0b]/10 flex items-center justify-center mb-4">
              <svg className="w-6 h-6 text-[#f59e0b]" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2">
                <path d="M3 21h18M3 10h18M5 6l7-3 7 3M4 10v11M20 10v11" />
                <path d="M8 14v3M12 14v3M16 14v3" />
              </svg>
            </div>
            <h2 className="text-xl font-bold text-text-primary mb-2">In-House Driver</h2>
            <p className="text-text-muted text-xs mb-1">Midwest Medical Supplies</p>
            <p className="text-text-secondary text-sm leading-relaxed">
              Manage your own fleet and deliveries
            </p>
            <div className="mt-6 text-[#f59e0b] text-sm font-medium opacity-0 group-hover:opacity-100 transition-opacity">
              Enter →
            </div>
          </button>
        </div>

        {/* DF Admin — smaller, separate row */}
        <div className="mt-6 flex justify-center">
          <button
            onClick={() => setRole('dfadmin')}
            className="group bg-[#0d0c2c] border border-[#0d0c2c] rounded-lg px-8 py-4 text-left transition-all hover:border-brand-cyan hover:shadow-md hover:-translate-y-px flex items-center gap-4"
          >
            <div className="w-10 h-10 rounded-lg bg-brand-cyan/20 flex items-center justify-center">
              <svg className="w-5 h-5 text-brand-cyan" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2">
                <path d="M12 15v2m-6 4h12a2 2 0 0 0 2-2v-6a2 2 0 0 0-2-2H6a2 2 0 0 0-2 2v6a2 2 0 0 0 2 2zm10-10V7a4 4 0 0 0-8 0v4h8z" />
              </svg>
            </div>
            <div>
              <h2 className="text-base font-bold text-white">DF Admin</h2>
              <p className="text-gray-400 text-xs">Platform configuration & feature management</p>
            </div>
            <span className="text-brand-cyan text-sm font-medium opacity-0 group-hover:opacity-100 transition-opacity ml-4">Enter →</span>
          </button>
        </div>

        <p className="text-center text-text-muted text-xs mt-8">
          Select a role to continue. You can switch roles anytime from the top bar.
        </p>
      </div>
    </div>
  );
}
