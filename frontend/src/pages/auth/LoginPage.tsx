import { Navigate } from 'react-router-dom';
import { AuthLayout } from '../../components/layout/AuthLayout';
import { LoginForm } from '../../features/auth/components/LoginForm';
import { useAuth } from '../../context/AuthContext';
import misLogo from '../../assets/mis-logo.svg';

export function LoginPage() {
  const { isAuthenticated } = useAuth();

  if (isAuthenticated) {
    return <Navigate to="/dashboard" replace />;
  }

  return (
    <AuthLayout>
      <div className="rounded-2xl border border-white/80 bg-white p-6 shadow-panel sm:p-8">
        <div className="mb-8">
          <img src={misLogo} alt="MIS Collection Firm" className="mb-5 h-20 w-auto md:hidden" />
          <p className="text-sm font-semibold uppercase text-mis-primary">MIS Collection Firm</p>
          <h2 className="mt-3 text-3xl font-bold text-mis-navy">Welcome Back</h2>
          <p className="mt-2 text-sm text-slate-500">Sign in to your MIS account</p>
        </div>

        <LoginForm />

        <div className="mt-8 border-t border-mis-border pt-5 text-center">
          <p className="text-sm font-semibold text-mis-navy">MIS Collection Firm</p>
          <p className="mt-1 text-sm text-slate-500">Internal Management System</p>
        </div>
      </div>

      <p className="mt-6 text-center text-sm text-slate-500">&copy; MIS Collection Firm</p>
    </AuthLayout>
  );
}
