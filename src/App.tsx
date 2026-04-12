import { Routes, Route, Navigate } from 'react-router-dom';
import { useState } from 'react';
import { useRole } from '@/context/RoleContext';
import { TenantConfigProvider } from '@/context/TenantConfigContext';
import AppLayout from '@/components/Layout/AppLayout';
import RoleSelect from '@/pages/RoleSelect';
import UpgradeModal from '@/components/common/UpgradeModal';
import TenantConfigPage from '@/pages/settings/TenantConfig';

// Public pages
import Login from '@/pages/Login';

// Applicant Portal (public-facing mobile flow)
import ApplicantPortalMobile from '@/pages/applicant/ApplicantPortal';

// Portal Pages (Courier self-service — logged-in couriers, desktop shell)
import PortalLayout from '@/components/portal/PortalLayout';
import PortalDashboard from '@/pages/portal/Dashboard';
import PortalSchedule from '@/pages/portal/Schedule';
import PortalRuns from '@/pages/portal/Runs';
import PortalInvoicing from '@/pages/portal/Invoicing';
import PortalContractors from '@/pages/portal/Contractors';
import PortalReports from '@/pages/portal/Reports';

// Courier Mobile App (bottom-nav mobile layout)
import CourierLayout from '@/pages/courier/CourierLayout';
import CourierLogin from '@/pages/courier/CourierLogin';
import CourierDashboard from '@/pages/courier/CourierDashboard';
import MyRuns from '@/pages/courier/MyRuns';
import CourierSchedule from '@/pages/courier/CourierSchedule';
import CourierDocuments from '@/pages/courier/CourierDocuments';
import CourierTraining from '@/pages/courier/CourierTraining';
import CourierInvoicing from '@/pages/courier/CourierInvoicing';
import CourierContractors from '@/pages/courier/CourierContractors';
import CourierSettings from '@/pages/courier/CourierSettings';

// NP Admin Pages
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
import ComplianceHub from '@/pages/np/ComplianceHub';
import RecruitmentPipeline from '@/pages/np/RecruitmentPipeline';
import ApplicantDetail from '@/pages/np/ApplicantDetail';
import RecruitmentStageSettings from '@/pages/np/RecruitmentStageSettings';
import ContractSettings from '@/pages/np/ContractSettings';
import NpApplicantPortal from '@/pages/np/ApplicantPortal';
import RecruitmentAdvertising from '@/pages/np/RecruitmentAdvertising';
import PortalUrl from '@/pages/np/PortalUrl';
import Scheduling from '@/pages/np/Scheduling';
import RegistrationSettings from '@/pages/np/RegistrationSettings';
import FleetManagement from '@/pages/np/FleetManagement';
import ComplianceProfiles from '@/pages/np/ComplianceProfiles';
import OpenforceActivity from '@/pages/np/OpenforceActivity';
import Operations from '@/pages/np/Operations';
import QuizBuilder from '@/pages/np/QuizBuilder';
// QuizPlayer is used inline (requires quizId + courierId props) — not a standalone route
// import QuizPlayer from '@/pages/np/QuizPlayer';
import FlowBuilder from '@/pages/np/FlowBuilder';
import DocumentManagement from '@/pages/np/DocumentManagement';
import SetupPassword from '@/pages/np/SetupPassword';

export default function App() {
  const { role } = useRole();
  const [upgradeOpen, setUpgradeOpen] = useState(false);
  const [selectedCourierId, setSelectedCourierId] = useState<number | null>(null);

  return (
    <TenantConfigProvider>
      <Routes>
        {/* Public routes */}
        <Route path="/login" element={<Login />} />
        <Route path="/setup-password" element={<SetupPassword />} />

        {/* Public applicant portal — no auth required */}
        <Route path="/apply" element={<ApplicantPortalMobile />} />
        <Route path="/apply/:tenantSlug" element={<ApplicantPortalMobile />} />
        {/* Legacy paths */}
        <Route path="/portal/apply" element={<ApplicantPortalMobile />} />
        <Route path="/portal/apply/:tenantSlug" element={<ApplicantPortalMobile />} />

        {/* Courier Mobile App (bottom-nav, mobile-first) */}
        <Route path="/courier" element={<CourierLayout />}>
          <Route path="login" element={<CourierLogin />} />
          <Route path="dashboard" element={<CourierDashboard />} />
          <Route path="runs" element={<MyRuns />} />
          <Route path="schedule" element={<CourierSchedule />} />
          <Route path="documents" element={<CourierDocuments />} />
          <Route path="training" element={<CourierTraining />} />
          <Route path="invoicing" element={<CourierInvoicing />} />
          <Route path="contractors" element={<CourierContractors />} />
          <Route path="settings" element={<CourierSettings />} />
          <Route index element={<Navigate to="/courier/dashboard" replace />} />
        </Route>

        {/* Courier Operations Portal — logged-in couriers, desktop admin shell */}
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

        {/* Role selector — shown when no role selected */}
        <Route path="/select-role" element={<RoleSelect />} />

        {/* NP Admin — full application shell */}
        <Route element={<AppLayout onUpgrade={() => setUpgradeOpen(true)} selectedCourierId={selectedCourierId} />}>
          {/* NP Admin routes (role: np or dfadmin) */}
          <Route index element={<NpDashboard onUpgrade={() => setUpgradeOpen(true)} />} />
          <Route path="fleet" element={<FleetOverview onSelectCourier={setSelectedCourierId} />} />
          <Route path="fleet/add" element={<AddCourier />} />
          <Route path="fleet/import" element={<CourierImport />} />
          <Route path="fleet/links" element={<CourierPortalLinks />} />
          <Route path="courier/:id" element={<CourierSetup onSelectCourier={setSelectedCourierId} />} />
          <Route path="users" element={<Users />} />
          <Route path="users/import" element={<UserImport />} />
          <Route path="reports" element={<Reports />} />
          <Route path="operations" element={<Operations />} />

          {/* Compliance */}
          <Route path="compliance" element={<ComplianceHub />} />
          <Route path="compliance-profiles" element={<ComplianceHub initialTab="profiles" />} />
          <Route path="driver-approval" element={<ComplianceHub initialTab="approval" />} />

          {/* Recruitment */}
          <Route path="recruitment" element={<RecruitmentPipeline />} />
          <Route path="recruitment/:id" element={<ApplicantDetail />} />
          <Route path="recruitment/portal-url" element={<PortalUrl />} />

          {/* Fleet management */}
          <Route path="fleet-management" element={<FleetManagement />} />
          <Route path="fleet-management/:id" element={<FleetManagement />} />

          {/* Scheduling */}
          <Route path="scheduling" element={<Scheduling />} />

          {/* Quiz / Training */}
          <Route path="quiz-builder" element={<QuizBuilder />} />
          {/* QuizPlayer requires quizId + courierId — rendered inline from QuizBuilder/DocumentTypeSettings */}

          {/* Flow / Document builder */}
          <Route path="flow-builder" element={<FlowBuilder />} />
          <Route path="documents" element={<DocumentManagement />} />

          {/* Settings */}
          <Route path="settings" element={<NpSettings onUpgrade={() => setUpgradeOpen(true)} />} />
          <Route path="settings/document-types" element={<ComplianceHub initialTab="documents" standalone />} />
          <Route path="settings/recruitment-stages" element={<RecruitmentStageSettings />} />
          <Route path="settings/contracts" element={<ContractSettings />} />
          <Route path="settings/recruitment-ads" element={<RecruitmentAdvertising />} />
          <Route path="settings/registration" element={<RegistrationSettings />} />
          <Route path="settings/openforce-activity" element={<OpenforceActivity />} />
          <Route path="settings/applicant-portal" element={<NpApplicantPortal />} />
          <Route path="settings/quiz-builder" element={<QuizBuilder />} />
          <Route path="settings/flow-builder" element={<FlowBuilder />} />
          <Route path="settings/tenant-config" element={<TenantConfigPage />} />

          <Route path="*" element={<Navigate to="/" replace />} />
        </Route>

        {/* Root redirect */}
        <Route path="/" element={<Navigate to="/login" replace />} />
      </Routes>
      <UpgradeModal open={upgradeOpen} onClose={() => setUpgradeOpen(false)} />
    </TenantConfigProvider>
  );
}
