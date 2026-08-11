import { Activity, BarChart3, CalendarDays, CalendarCheck2, FileText, Gauge, Languages, LogOut, Menu, ScrollText, UserRoundX, X, Database, UsersRound } from 'lucide-react';
import { useState } from 'react';
import { NavLink, Outlet, useNavigate } from 'react-router-dom';
import { useAuth } from '../../context/AuthContext';
import misLogo from '../../assets/mis-logo.svg';
import { useLocalization } from '../../context/LocalizationContext';
import type { TranslationKey } from '../../localization/translations';

const links = [
  { to: '/hr/dashboard', label: 'dashboard', icon: Gauge },
  { to: '/hr/employees', label: 'employees', icon: UsersRound },
  { to: '/hr/attendance', label: 'attendance', icon: CalendarCheck2 },
  { to: '/hr/leaves', label: 'leaves', icon: CalendarDays },
  { to: '/hr/delegations', label: 'delegations', icon: ScrollText },
  { to: '/hr/absences', label: 'companyAbsences', icon: UserRoundX },
  { to: '/hr/employee-documents', label: 'employeeDocuments', icon: FileText },
  { to: '/hr/master', label: 'master', icon: Database },
  { to: '/hr/calendar', label: 'workingCalendar', icon: CalendarDays },
  { to: '/hr/reports', label: 'reports', icon: BarChart3 },
  { to: '/hr/audit', label: 'auditHistory', icon: Activity },
];

export function HrLayout() {
  const [open, setOpen] = useState(false);
  const [languageOpen, setLanguageOpen] = useState(false);
  const { isRtl, language, setLanguage, t } = useLocalization();
  const { logout, user } = useAuth();
  const navigate = useNavigate();
  function signOut() { logout(); navigate('/login', { replace: true }); }

  return (
    <div className="min-h-screen bg-mis-surface text-mis-ink">
      {open && <button aria-label={t('closeNavigation')} className="fixed inset-0 z-30 bg-mis-ink/40 lg:hidden" onClick={() => setOpen(false)} type="button" />}
      <aside style={{ insetInlineStart: 0, borderInlineEndWidth: 1 }} className={`fixed inset-y-0 z-40 flex w-72 flex-col border-mis-border bg-white transition-transform lg:translate-x-0 ${open ? 'translate-x-0' : isRtl ? 'translate-x-full' : '-translate-x-full'}`}>
        <div className="flex h-24 items-center justify-between border-b border-mis-border px-6">
          <img src={misLogo} alt={t('companyLogoAlt')} className="h-16 w-auto" />
          <button aria-label={t('closeNavigation')} className="rounded-lg p-2 text-slate-500 lg:hidden" onClick={() => setOpen(false)}><X /></button>
        </div>
        <div className="px-6 pb-3 pt-6"><p className="text-xs font-bold uppercase tracking-widest text-mis-primary">{t('humanResources')}</p><p className="mt-1 text-sm text-slate-500">{t('hrDepartment')}</p></div>
        <nav className="min-h-0 flex-1 space-y-1 overflow-y-auto px-4 py-3" aria-label={t('hrNavigation')}>
          {links.map(({ to, label, icon: Icon }) => <NavLink key={to} to={to} onClick={() => setOpen(false)} className={({ isActive }) => `flex min-h-12 items-center gap-3 rounded-xl px-4 text-sm font-semibold transition ${isActive ? 'bg-mis-pale text-mis-deep' : 'text-slate-600 hover:bg-slate-50 hover:text-mis-primary'}`}><Icon className="h-5 w-5" aria-hidden="true" /><span>{t(label as TranslationKey)}</span></NavLink>)}
          <button className="flex min-h-12 w-full items-center gap-3 rounded-xl px-4 text-sm font-semibold text-slate-600 hover:bg-slate-50 hover:text-mis-primary" onClick={() => setLanguageOpen((value) => !value)} type="button"><Languages className="h-5 w-5" /><span className="flex-1 text-start">{t('language')}</span><span className="text-xs text-slate-400">{language === 'ar' ? 'العربية' : 'EN'}</span></button>
          {languageOpen && <div className="ms-4 space-y-1 border-s border-mis-border ps-3"><button className={`block w-full rounded-lg px-3 py-2 text-start text-sm ${language === 'en' ? 'bg-mis-pale font-semibold text-mis-deep' : 'text-slate-600'}`} onClick={() => setLanguage('en')}>{t('english')}</button><button className={`block w-full rounded-lg px-3 py-2 text-start text-sm ${language === 'ar' ? 'bg-mis-pale font-semibold text-mis-deep' : 'text-slate-600'}`} onClick={() => setLanguage('ar')}>{t('arabic')}</button></div>}
        </nav>
        <div className="border-t border-mis-border p-4">
          <div className="mb-3 px-3"><p className="truncate text-sm font-semibold text-mis-navy">{user?.fullName}</p><p className="text-xs text-slate-500">{t('hrDepartment')}</p></div>
          <button className="flex w-full items-center gap-3 rounded-xl px-3 py-3 text-sm font-semibold text-slate-600 hover:bg-slate-50 hover:text-mis-primary" onClick={signOut} type="button"><LogOut className="h-5 w-5" />{t('logout')}</button>
        </div>
      </aside>
      <div style={{ [isRtl ? 'paddingRight' : 'paddingLeft']: 'var(--hr-sidebar-space)' }} className="hr-content">
        <header className="sticky top-0 z-20 flex h-20 items-center border-b border-mis-border bg-white/95 px-5 backdrop-blur sm:px-8">
          <button aria-label={t('openNavigation')} className="me-4 rounded-lg border border-mis-border p-2 text-mis-navy lg:hidden" onClick={() => setOpen(true)} type="button"><Menu /></button>
          <div><p className="text-xs font-semibold uppercase tracking-wide text-mis-primary">{t('collectionFirm')}</p><p className="mt-1 font-bold text-mis-navy">{t('humanResources')}</p></div>
        </header>
        <main className="p-5 sm:p-8"><Outlet /></main>
      </div>
    </div>
  );
}
