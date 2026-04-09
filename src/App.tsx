import { Routes, Route, Navigate } from 'react-router-dom';
import { useState } from 'react';

// Existing pages
import ApplicantPortal from '@/pages/applicant/ApplicantPortal';
import CourierTraining from '@/pages/courier/CourierTraining';
import FlowBuilder from '@/pages/np/FlowBuilder';
import QuizBuilder from '@/pages/np/QuizBuilder';
import AdminSettings from '@/pages/np/AdminSettings';
import DocumentManagement from '@/pages/np/DocumentManagement';
import SetupPassword from '@/pages/np/SetupPassword';
import Login from '@/pages/Login';

export default function App() {
  return (
    <Routes>
      {/* Login */}
      <Route path="/login" element={<Login />} />

      {/* Public applicant portal */}
      <Route path="/portal/apply" element={<ApplicantPortal />} />
      <Route path="/portal/apply/:tenantSlug" element={<ApplicantPortal />} />

      {/* Courier training */}
      <Route path="/portal/training" element={<CourierTraining />} />

      {/* NP Admin pages */}
      <Route path="/admin/flow-builder" element={<FlowBuilder />} />
      <Route path="/admin/quiz-builder" element={<QuizBuilder />} />
      <Route path="/admin/settings" element={<AdminSettings />} />
      <Route path="/admin/documents" element={<DocumentManagement />} />
      <Route path="/admin/setup-password" element={<SetupPassword />} />

      {/* Default redirect */}
      <Route path="/" element={<Navigate to="/login" replace />} />
      <Route path="*" element={<Navigate to="/login" replace />} />
    </Routes>
  );
}
