import { lazy, Suspense } from 'react';
import { Navigate, Route, Routes } from 'react-router-dom';
import { LoginPage } from '../pages/auth/LoginPage';
import { HrLayout } from '../components/layout/HrLayout';
import { LoadingSpinner } from '../components/common/LoadingSpinner';
import { UnauthorizedPage } from '../pages/UnauthorizedPage';
import { DepartmentHome } from './DepartmentHome';
import { ProtectedRoute } from './ProtectedRoute';

const HrDashboardPage = lazy(() => import('../pages/hr/HrDashboardPage').then((module) => ({ default: module.HrDashboardPage })));
const HrEmployeesPage = lazy(() => import('../pages/hr/HrEmployeesPage').then((module) => ({ default: module.HrEmployeesPage })));
const HrEmployeeProfilePage = lazy(() => import('../pages/hr/HrEmployeeProfilePage').then((module) => ({ default: module.HrEmployeeProfilePage })));
const HrAttendancePage = lazy(() => import('../pages/hr/HrAttendancePage').then((module) => ({ default: module.HrAttendancePage })));
const HrAttendanceImportPage = lazy(() => import('../pages/hr/HrAttendanceImportPage').then((module) => ({ default: module.HrAttendanceImportPage })));
const HrLeavesPage = lazy(() => import('../pages/hr/HrLeavesPage').then((module) => ({ default: module.HrLeavesPage })));
const HrCalendarPage = lazy(() => import('../pages/hr/HrCalendarPage').then((module) => ({ default: module.HrCalendarPage })));
const HrReportsPage = lazy(() => import('../pages/hr/HrReportsPage').then((module) => ({ default: module.HrReportsPage })));
const HrDelegationsPage = lazy(() => import('../pages/hr/HrDelegationsPage').then((module) => ({ default: module.HrDelegationsPage })));
const HrAbsencesPage = lazy(() => import('../pages/hr/HrAbsencesPage').then((module) => ({ default: module.HrAbsencesPage })));
const HrEmployeeDocumentsPage = lazy(() => import('../pages/hr/HrEmployeeDocumentsPage').then((module) => ({ default: module.HrEmployeeDocumentsPage })));
const HrMasterPage = lazy(() => import('../pages/hr/HrMasterPage').then((module) => ({ default: module.HrMasterPage })));
const HrAuditPage = lazy(() => import('../pages/hr/HrAuditPage').then((module) => ({ default: module.HrAuditPage })));

function HrRouteBoundary() {
  return <Suspense fallback={<div className="flex min-h-[420px] items-center justify-center"><LoadingSpinner /></div>}><HrLayout /></Suspense>;
}

export function AppRoutes() {
  return (
    <Routes>
      <Route path="/" element={<DepartmentHome />} />
      <Route path="/login" element={<LoginPage />} />
      <Route element={<ProtectedRoute department="HR" />}>
        <Route path="/hr" element={<Navigate to="/hr/dashboard" replace />} />
        <Route path="/hr" element={<HrRouteBoundary />}>
          <Route path="dashboard" element={<HrDashboardPage />} />
          <Route path="employees" element={<HrEmployeesPage />} />
          <Route path="employees/:id" element={<HrEmployeeProfilePage />} />
          <Route path="attendance" element={<HrAttendancePage />} />
          <Route path="attendance/import" element={<HrAttendanceImportPage />} />
          <Route path="leaves" element={<HrLeavesPage />} />
          <Route path="calendar" element={<HrCalendarPage />} />
          <Route path="reports" element={<HrReportsPage />} />
          <Route path="delegations" element={<HrDelegationsPage />} />
          <Route path="absences" element={<HrAbsencesPage />} />
          <Route path="employee-documents" element={<HrEmployeeDocumentsPage />} />
          <Route path="audit" element={<HrAuditPage />} />
          <Route path="master" element={<HrMasterPage />} />
        </Route>
      </Route>
      <Route path="/unauthorized" element={<UnauthorizedPage />} />
      <Route path="*" element={<Navigate to="/" replace />} />
    </Routes>
  );
}
