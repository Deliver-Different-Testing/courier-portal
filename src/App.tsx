import { Routes, Route, Navigate } from 'react-router-dom';
import { useState } from 'react';
import { useRole } from '@/context/RoleContext';
import { TenantConfigProvider } from '@/context/TenantConfigContext';
import AppLayout from '@/components/Layout/AppLayout';
import RoleSelect from '@/pages/RoleSelect';
import UpgradeModal from '@/components/common/UpgradeModal';
import TenantConfigPage from '@/pages/settings/TenantConfig';

// Portal Pages (Courier-facing)
import PortalLayout from '@/components/portal/PortalLayout';
import PortalDashboard from '@/pages/portal/Dashboard';
import PortalSchedule from '@/pages/portal/Schedule';
import PortalRuns from '@/pages/portal/Runs';
import PortalInvoicing from '@/pages/portal/Invoicing';
import PortalContractors from '@/pages/portal/Contractors';
import PortalReports from '@/pages/portal/Reports';
import CourierTraining from '@/pages/courier/CourierTraining';

// NP Pages
import NpDashboard from '@/pages/np/Dashboard';
import FleetOverview from '@/pages/np/FleetOverview';
import AddCourier from '@/pages/np/AddCourier';
import CourierPortalLinks from '@/pages/np/CourierPortalLinks';
import CourierSetup from '@/pages/np/CourierSetup';
import CourierImport from '@/pages/np/CourierImport';
import Users from '@/pages/np/Users';
import UserImport from '@/pages/np/UserImport';
import Reports from '@/pages/np/Reports';
import NpSettings from '@/pages/np/Settings';
import DocumentTypeSettings from '@/pages/np/DocumentTypeSettings';
import ComplianceDashboard from '@/pages/np/ComplianceDashboard';
import ComplianceHub from '@/pages/np/ComplianceHub';
import RecruitmentPipeline from '@/pages/np/RecruitmentPipeline';
import ApplicantDetail from '@/pages/np/ApplicantDetail';
import RecruitmentStageSettings from '@/pages/np/RecruitmentStageSettings';
import ContractSettings from '@/pages/np/ContractSettings';
import ApplicantPortal from '@/pages/np/ApplicantPortal';
import RecruitmentAdvertising from '@/pages/np/RecruitmentAdvertising';
import PortalUrl from '@/pages/np/PortalUrl';
import Scheduling from '@/pages/np/Scheduling';
import RegistrationSettings from '@/pages/np/RegistrationSettings';
import FleetManagement from '@/pages/np/FleetManagement';
import ComplianceProfiles from '@/pages/np/ComplianceProfiles';
import OpenforceActivity from '@/pages/np/OpenforceActivity';

// ID Pages
import IDDashboard from '@/pages/id/Dashboard';
import IDDeliveries from '@/pages/id/Deliveries';
import IDMyFleet from '@/pages/id/MyFleet';
import IDReports from '@/pages/id/Reports';
import IDSettings from '@/pages/id/Settings';

// Tenant Pages
import { Dashboard as TenantDashboard } from '@/pages/tenant/Dashboard';
import { AgentDiscovery } from '@/pages/tenant/AgentDiscovery';
import { AgentList } from '@/pages/tenant/AgentList';
import { AgentDetail } from '@/pages/tenant/AgentDetail';
import { AgentOnboarding } from '@/pages/tenant/AgentOnboarding';
import { NpManagement } from '@/pages/tenant/NpManagement';
import { QuoteRequests } from '@/pages/tenant/QuoteRequests';
import { AssociationStats } from '@/pages/tenant/AssociationStats';
import { AgentImport } from '@/pages/tenant/AgentImport';
import { DriverApproval } from '@/pages/tenant/DriverApproval';

export default function App() {
  const { role } = useRole();
  const [upgradeOpen, setUpgradeOpen] = useState(false);
  const [selectedCourierId, setSelectedCourierId] = useState<number | null>(null);

  if (!role) {
    return <RoleSelect />;
  }

  return (
    <TenantConfigProvider>
      <Routes>
        {/* Public portal routes — no auth required */}
        <Route path="/portal/apply" element={<ApplicantPortal />} />
        <Route path="/portal/apply/:tenantSlug" element={<ApplicantPortal />} />

        {/* Courier Operations Portal — logged-in couriers */}
        <Route path="/portal" element={<PortalLayout />}>
          <Route path="dashboard" element={<PortalDashboard />} />
          <Route path="schedule" element={<PortalSchedule />} />
          <Route path="runs" element={<PortalRuns />} />
          <Route path="training" element={<CourierTraining />} />
          <Route path="invoicing" element={<PortalInvoicing />} />
          <Route path="contractors" element={<PortalContractors />} />
          <Route path="reports" element={<PortalReports />} />
          <Route index element={<Navigate to="/portal/dashboard" replace />} />
        </Route>
        <Route element={<AppLayout onUpgrade={() => setUpgradeOpen(true)} selectedCourierId={selectedCourierId} />}>
          {role === 'np' && (
            <>
              <Route index element={<NpDashboard onUpgrade={() => setUpgradeOpen(true)} />} />
              <Route path="fleet" element={<FleetOverview onSelectCourier={setSelectedCourierId} />} />
              <Route path="fleet/add" element={<AddCourier />} />
              <Route path="fleet/import" element={<CourierImport />} />
              <Route path="fleet/links" element={<CourierPortalLinks />} />
              <Route path="courier/:id" element={<CourierSetup onSelectCourier={setSelectedCourierId} />} />
              <Route path="users" element={<Users />} />
              <Route path="users/import" element={<UserImport />} />
              <Route path="reports" element={<Reports />} />
              <Route path="compliance" element={<ComplianceHub />} />
              <Route path="compliance-profiles" element={<ComplianceHub initialTab="profiles" />} />
              <Route path="driver-approval" element={<ComplianceHub initialTab="approval" />} />
              <Route path="recruitment" element={<RecruitmentPipeline />} />
              <Route path="recruitment/:id" element={<ApplicantDetail />} />
              <Route path="settings" element={<NpSettings onUpgrade={() => setUpgradeOpen(true)} />} />
              <Route path="settings/document-types" element={<ComplianceHub initialTab="documents" standalone />} />
              <Route path="settings/recruitment-stages" element={<RecruitmentStageSettings />} />
              <Route path="settings/contracts" element={<ContractSettings />} />
              <Route path="settings/recruitment-ads" element={<RecruitmentAdvertising />} />
              <Route path="settings/registration" element={<RegistrationSettings />} />
              <Route path="settings/openforce-activity" element={<OpenforceActivity />} />
              <Route path="recruitment/portal-url" element={<PortalUrl />} />
              <Route path="fleet-management" element={<FleetManagement />} />
              <Route path="fleet-management/:id" element={<FleetManagement />} />
              <Route path="scheduling" element={<Scheduling />} />
            </>
          )}
          {role === 'dfadmin' && (
            <>
              <Route index element={<NpDashboard onUpgrade={() => setUpgradeOpen(true)} />} />
              <Route path="fleet" element={<FleetOverview onSelectCourier={setSelectedCourierId} />} />
              <Route path="fleet/add" element={<AddCourier />} />
              <Route path="fleet/import" element={<CourierImport />} />
              <Route path="fleet/links" element={<CourierPortalLinks />} />
              <Route path="courier/:id" element={<CourierSetup onSelectCourier={setSelectedCourierId} />} />
              <Route path="users" element={<Users />} />
              <Route path="users/import" element={<UserImport />} />
              <Route path="reports" element={<Reports />} />
              <Route path="compliance" element={<ComplianceHub />} />
              <Route path="compliance-profiles" element={<ComplianceHub initialTab="profiles" />} />
              <Route path="driver-approval" element={<ComplianceHub initialTab="approval" />} />
              <Route path="recruitment" element={<RecruitmentPipeline />} />
              <Route path="recruitment/:id" element={<ApplicantDetail />} />
              <Route path="settings" element={<NpSettings onUpgrade={() => setUpgradeOpen(true)} isDfAdmin />} />
              <Route path="settings/tenant-config" element={<TenantConfigPage />} />
              <Route path="settings/document-types" element={<ComplianceHub initialTab="documents" standalone />} />
              <Route path="settings/recruitment-stages" element={<RecruitmentStageSettings />} />
              <Route path="settings/contracts" element={<ContractSettings />} />
              <Route path="settings/recruitment-ads" element={<RecruitmentAdvertising />} />
              <Route path="settings/registration" element={<RegistrationSettings />} />
              <Route path="settings/openforce-activity" element={<OpenforceActivity />} />
              <Route path="recruitment/portal-url" element={<PortalUrl />} />
              <Route path="fleet-management" element={<FleetManagement />} />
              <Route path="fleet-management/:id" element={<FleetManagement />} />
              <Route path="scheduling" element={<Scheduling />} />
            </>
          )}
          {role === 'tenant' && (
            <>
              <Route index element={<TenantDashboard />} />
              <Route path="discovery" element={<AgentDiscovery />} />
              <Route path="agents" element={<AgentList />} />
              <Route path="agents/:id" element={<AgentDetail />} />
              <Route path="onboarding" element={<AgentOnboarding />} />
              <Route path="fleet" element={<FleetOverview onSelectCourier={setSelectedCourierId} />} />
              <Route path="fleet/add" element={<AddCourier />} />
              <Route path="fleet/import" element={<CourierImport />} />
              <Route path="courier/:id" element={<CourierSetup onSelectCourier={setSelectedCourierId} />} />
              <Route path="compliance" element={<ComplianceHub />} />
              <Route path="driver-approval" element={<ComplianceHub initialTab="approval" />} />
              <Route path="settings/document-types" element={<ComplianceHub initialTab="documents" standalone />} />
              <Route path="settings/recruitment-stages" element={<RecruitmentStageSettings />} />
              <Route path="settings/contracts" element={<ContractSettings />} />
              <Route path="settings/recruitment-ads" element={<RecruitmentAdvertising />} />
              <Route path="settings/registration" element={<RegistrationSettings />} />
              <Route path="recruitment/portal-url" element={<PortalUrl />} />
              <Route path="recruitment" element={<RecruitmentPipeline />} />
              <Route path="recruitment/:id" element={<ApplicantDetail />} />
              <Route path="compliance-profiles" element={<ComplianceHub initialTab="profiles" />} />
              <Route path="np-management" element={<NpManagement />} />
              <Route path="quotes" element={<QuoteRequests />} />
              <Route path="import" element={<AgentImport />} />
              <Route path="associations" element={<AssociationStats />} />
              <Route path="fleet-management" element={<FleetManagement />} />
              <Route path="fleet-management/:id" element={<FleetManagement />} />
              <Route path="scheduling" element={<Scheduling />} />
              <Route path="settings" element={<div className="text-text-primary"><h2 className="text-xl font-bold mb-4">Settings</h2><p className="text-text-secondary">Tenant settings coming soon.</p></div>} />
            </>
          )}
          {role === 'id' && (
            <>
              <Route index element={<IDDashboard />} />
              <Route path="deliveries" element={<IDDeliveries />} />
              <Route path="fleet" element={<FleetOverview onSelectCourier={setSelectedCourierId} />} />
              <Route path="fleet/add" element={<AddCourier />} />
              <Route path="courier/:id" element={<CourierSetup onSelectCourier={setSelectedCourierId} />} />
              <Route path="reports" element={<IDReports />} />
              <Route path="settings" element={<IDSettings />} />
            </>
          )}
          <Route path="*" element={<Navigate to="/" replace />} />
        </Route>
      </Routes>
      <UpgradeModal open={upgradeOpen} onClose={() => setUpgradeOpen(false)} />
    </TenantConfigProvider>
  );
}
