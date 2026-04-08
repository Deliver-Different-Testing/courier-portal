import { useState } from 'react';
import ComplianceDashboard from './ComplianceDashboard';
import ComplianceProfiles from './ComplianceProfiles';
import DocumentTypeSettings from './DocumentTypeSettings';
import { DriverApproval } from '@/pages/tenant/DriverApproval';
import { useRole } from '@/context/RoleContext';
import { driverApprovalService } from '@/services/np_driverApprovalService';

type Tab = 'dashboard' | 'documents' | 'profiles' | 'approval';

interface TabDef {
  id: Tab;
  label: string;
  badge?: number;
}

export default function ComplianceHub({ initialTab, standalone }: { initialTab?: Tab; standalone?: boolean }) {
  const [activeTab, setActiveTab] = useState<Tab>(initialTab || 'dashboard');
  const { role } = useRole();
  const pendingCount = driverApprovalService.getPendingCount();

  const tabs: TabDef[] = [
    { id: 'dashboard', label: 'Dashboard' },
    { id: 'approval', label: 'Driver Approval', badge: pendingCount > 0 ? pendingCount : undefined },
    { id: 'documents', label: 'Documents' },
    { id: 'profiles', label: 'Profiles' },
  ];

  const visibleTabs = tabs;

  return (
    <div className="space-y-6">
      {/* Tab Bar — hidden in standalone mode */}
      {!standalone && <div className="flex gap-1 bg-white rounded-lg border border-border p-1">
        {visibleTabs.map(tab => (
          <button
            key={tab.id}
            onClick={() => setActiveTab(tab.id)}
            className={`flex items-center gap-2 px-5 py-2.5 rounded-md text-sm font-medium transition-all ${
              activeTab === tab.id
                ? 'bg-[#0d0c2c] text-white shadow-sm'
                : 'text-text-secondary hover:bg-gray-50'
            }`}
          >
            {tab.label}
            {tab.badge && (
              <span className={`text-[10px] font-bold px-1.5 py-0.5 rounded-full ${
                activeTab === tab.id
                  ? 'bg-amber-400 text-[#0d0c2c]'
                  : 'bg-amber-100 text-amber-700'
              }`}>
                {tab.badge}
              </span>
            )}
          </button>
        ))}
      </div>}

      {/* Tab Content */}
      {activeTab === 'dashboard' && <ComplianceDashboard />}
      {activeTab === 'documents' && <DocumentTypeSettings />}
      {activeTab === 'profiles' && <ComplianceProfiles />}
      {activeTab === 'approval' && <DriverApproval />}
    </div>
  );
}
