import { Navigate, Route, Routes } from 'react-router-dom';
import { LoginPage } from '../pages/auth/LoginPage';
import { HrLayout } from '../components/layout/HrLayout';
import { HrPage } from '../pages/hr/HrPage';
import { HrDashboardPage } from '../pages/hr/HrDashboardPage';
import { HrEmployeesPage } from '../pages/hr/HrEmployeesPage';
import { HrAbsencesPage } from '../pages/hr/HrAbsencesPage';
import { UnauthorizedPage } from '../pages/UnauthorizedPage';
import { DepartmentHome } from './DepartmentHome';
import { ProtectedRoute } from './ProtectedRoute';

export function AppRoutes() {
  return (
    <Routes>
      <Route path="/" element={<DepartmentHome />} />
      <Route path="/login" element={<LoginPage />} />
      <Route element={<ProtectedRoute department="HR" />}>
        <Route path="/hr" element={<Navigate to="/hr/dashboard" replace />} />
        <Route path="/hr" element={<HrLayout />}>
          <Route path="dashboard" element={<HrDashboardPage />} />
          <Route path="employees" element={<HrEmployeesPage />} />
          <Route path="delegations" element={<HrPage titleKey="delegations" />} />
          <Route path="absences" element={<HrAbsencesPage />} />
          <Route path="employee-documents" element={<HrPage titleKey="employeeDocuments" />} />
          <Route path="master" element={<HrPage titleKey="master" />} />
        </Route>
      </Route>
      <Route path="/unauthorized" element={<UnauthorizedPage />} />
      <Route path="*" element={<Navigate to="/" replace />} />
    </Routes>
  );
}
