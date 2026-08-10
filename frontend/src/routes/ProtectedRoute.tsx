import { Navigate, Outlet, useLocation } from 'react-router-dom';
import { useAuth } from '../context/AuthContext';

interface ProtectedRouteProps {
  department?: string;
}

export function ProtectedRoute({ department }: ProtectedRouteProps) {
  const { isAuthenticated, user } = useAuth();
  const location = useLocation();

  if (!isAuthenticated) {
    return <Navigate to="/login" replace state={{ from: location }} />;
  }

  if (department && user?.department !== department) {
    return <Navigate to="/unauthorized" replace />;
  }

  return <Outlet />;
}
