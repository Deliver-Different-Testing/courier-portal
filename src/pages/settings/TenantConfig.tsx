import { useTenantConfig, TenantConfig } from '@/context/TenantConfigContext';

interface ToggleDef {
  key: keyof TenantConfig;
  label: string;
  description: string;
}

const TOGGLES: ToggleDef[] = [
  { key: 'directCouriersEnabled', label: 'Direct Couriers (IC)', description: 'Allow tenant to manage their own independent contractor couriers' },
  { key: 'agentsEnabled', label: 'Agents', description: 'Enable agent directory, discovery, onboarding and association management' },
  { key: 'networkPartnersEnabled', label: 'Network Partners', description: 'Enable NP management, driver approval and NP compliance features' },
  { key: 'courierRecruitmentEnabled', label: 'Courier Recruitment', description: 'Recruitment pipeline, applicant portal, and advertising' },
  { key: 'schedulingEnabled', label: 'Scheduling', description: 'Shift and schedule management for couriers' },
  { key: 'marketplaceEnabled', label: 'Marketplace', description: 'Quote requests and marketplace features' },
];

export default function TenantConfigPage() {
  const { config, updateConfig, resetConfig } = useTenantConfig();

  return (
    <div className="max-w-2xl">
      <div className="flex items-center justify-between mb-6">
        <div>
          <h2 className="text-xl font-bold text-text-primary">Tenant Configuration</h2>
          <p className="text-sm text-text-secondary mt-1">Toggle features for this tenant. Changes take effect immediately.</p>
        </div>
        <button
          onClick={resetConfig}
          className="text-sm text-text-muted hover:text-text-secondary px-3 py-1.5 rounded border border-border hover:border-border-hover transition-colors"
        >
          Reset to defaults
        </button>
      </div>

      <div className="space-y-1">
        {TOGGLES.map(t => (
          <label
            key={t.key}
            className="flex items-center justify-between p-4 rounded-lg border border-border hover:border-border-hover bg-white transition-colors cursor-pointer"
          >
            <div className="pr-4">
              <div className="text-sm font-medium text-text-primary">{t.label}</div>
              <div className="text-xs text-text-secondary mt-0.5">{t.description}</div>
            </div>
            <div className="relative flex-shrink-0">
              <input
                type="checkbox"
                checked={config[t.key]}
                onChange={() => updateConfig({ [t.key]: !config[t.key] })}
                className="sr-only peer"
              />
              <div className="w-11 h-6 bg-gray-200 peer-focus:ring-2 peer-focus:ring-brand-cyan/30 rounded-full peer peer-checked:bg-brand-cyan transition-colors" />
              <div className="absolute left-[2px] top-[2px] bg-white w-5 h-5 rounded-full shadow peer-checked:translate-x-5 transition-transform" />
            </div>
          </label>
        ))}
      </div>

      <div className="mt-6 p-4 rounded-lg bg-brand-cyan/5 border border-brand-cyan/20">
        <p className="text-xs text-text-secondary">
          <span className="font-medium text-brand-cyan">Note:</span> These toggles are stored locally for prototyping. In production they will be managed via the DF Admin API per tenant.
        </p>
      </div>
    </div>
  );
}
