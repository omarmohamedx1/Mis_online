import { Navigate } from 'react-router-dom';
import { AuthLayout } from '../../components/layout/AuthLayout';
import { LoginForm } from '../../features/auth/components/LoginForm';
import { useAuth } from '../../context/AuthContext';
import misLogo from '../../assets/mis-logo.svg';
import { useLocalization } from '../../context/LocalizationContext';

export function LoginPage() {
  const { isAuthenticated } = useAuth();
  const { t } = useLocalization();

  if (isAuthenticated) {
    return <Navigate to="/" replace />;
  }

  return (
    <AuthLayout>
      <div className="rounded-2xl border border-white/80 bg-white p-6 shadow-panel sm:p-8">
        <div className="mb-8">
          <img src={misLogo} alt={t('companyLogoAlt')} className="mb-5 h-20 w-auto md:hidden" />
          <p className="text-sm font-semibold uppercase text-mis-primary">{t('collectionFirm')}</p>
          <h2 className="mt-3 text-3xl font-bold text-mis-navy">{t('welcomeBack')}</h2>
          <p className="mt-2 text-sm text-slate-500">{t('signInSubtitle')}</p>
        </div>

        <LoginForm />

        <div className="mt-8 border-t border-mis-border pt-5 text-center">
          <p className="text-sm font-semibold text-mis-navy">{t('collectionFirm')}</p>
          <p className="mt-1 text-sm text-slate-500">{t('internalSystem')}</p>
        </div>
      </div>

      <p className="mt-6 text-center text-sm text-slate-500">&copy; {t('collectionFirm')}</p>
    </AuthLayout>
  );
}
