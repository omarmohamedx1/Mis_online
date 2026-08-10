import { LogOut, ShieldX } from 'lucide-react';
import { useAuth } from '../context/AuthContext';
import { useLocalization } from '../context/LocalizationContext';

export function UnauthorizedPage() {
  const { logout } = useAuth();
  const { t } = useLocalization();
  return (
    <main className="flex min-h-screen items-center justify-center bg-mis-surface p-5">
      <section className="w-full max-w-lg rounded-2xl border border-mis-border bg-white p-8 text-center shadow-panel">
        <ShieldX className="mx-auto h-12 w-12 text-mis-primary" aria-hidden="true" />
        <h1 className="mt-5 text-2xl font-bold text-mis-navy">{t('accessUnavailable')}</h1>
        <p className="mt-3 text-sm leading-6 text-slate-500">{t('accessHelp')}</p>
        <button className="mt-6 inline-flex items-center gap-2 rounded-lg bg-mis-primary px-4 py-3 text-sm font-semibold text-white hover:bg-mis-deep" onClick={logout} type="button">
          <LogOut className="h-4 w-4" aria-hidden="true" /> {t('signOut')}
        </button>
      </section>
    </main>
  );
}
