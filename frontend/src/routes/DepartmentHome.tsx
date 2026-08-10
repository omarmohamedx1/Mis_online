import { Navigate } from 'react-router-dom';
import { useAuth } from '../context/AuthContext';

export function DepartmentHome() {
  const { isAuthenticated, user } = useAuth();

  if (!isAuthenticated) return <Navigate to="/login" replace />;
  if (user?.department === 'HR') return <Navigate to="/hr/dashboard" replace />;
  return <Navigate to="/unauthorized" replace />;
}
