import { LogOut } from 'lucide-react';
import { useAuth } from '../../context/AuthContext';

export function DashboardPage() {
  const { logout, user } = useAuth();

  return (
    <main className="min-h-screen bg-mis-surface">
      <header className="border-b border-mis-border bg-white">
        <div className="mx-auto flex max-w-7xl items-center justify-between gap-4 px-5 py-4 sm:px-8">
          <div>
            <p className="text-sm font-semibold uppercase text-mis-primary">MIS Collection Firm</p>
            <h1 className="mt-1 text-xl font-bold text-mis-navy">Welcome to MIS</h1>
          </div>
          <button
            className="inline-flex h-10 items-center gap-2 rounded-lg border border-mis-border px-3 text-sm font-semibold text-slate-700 transition hover:border-mis-blue hover:text-mis-primary"
            onClick={logout}
            type="button"
          >
            <LogOut className="h-4 w-4" aria-hidden="true" />
            <span>Sign Out</span>
          </button>
        </div>
      </header>

      <section className="mx-auto max-w-7xl px-5 py-8 sm:px-8">
        <div className="rounded-xl border border-mis-border bg-white p-6 shadow-sm">
          <p className="text-sm font-semibold text-slate-500">Signed in as</p>
          <p className="mt-2 text-2xl font-bold text-mis-navy">{user?.fullName ?? 'MIS User'}</p>
          <p className="mt-2 text-sm text-slate-500">{user?.role ?? 'User'}</p>
        </div>
      </section>
    </main>
  );
}
