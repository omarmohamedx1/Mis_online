import { Activity, BarChart3, CalendarDays, CalendarCheck2, Database, FileText, Gauge, Languages, LogOut, Menu, PanelLeftClose, PanelLeftOpen, ScrollText, UserCircle, UserRoundX, UsersRound, X } from 'lucide-react';
import { useState } from 'react';
import { NavLink, Outlet, useNavigate } from 'react-router-dom';
import misLogo from '../../assets/mis-logo.svg';
import { useAuth } from '../../context/AuthContext';
import { useLocalization } from '../../context/LocalizationContext';
import type { TranslationKey } from '../../localization/translations';

const links = [
  { to: '/hr/dashboard', label: 'dashboard', icon: Gauge }, { to: '/hr/employees', label: 'employees', icon: UsersRound },
  { to: '/hr/attendance', label: 'attendance', icon: CalendarCheck2 }, { to: '/hr/leaves', label: 'leaves', icon: CalendarDays },
  { to: '/hr/delegations', label: 'delegations', icon: ScrollText }, { to: '/hr/absences', label: 'companyAbsences', icon: UserRoundX },
  { to: '/hr/employee-documents', label: 'employeeDocuments', icon: FileText }, { to: '/hr/master', label: 'master', icon: Database },
  { to: '/hr/calendar', label: 'workingCalendar', icon: CalendarDays }, { to: '/hr/reports', label: 'reports', icon: BarChart3 },
  { to: '/hr/audit', label: 'auditHistory', icon: Activity },
];

export function HrLayout() {
  const [open, setOpen] = useState(false); const [languageOpen, setLanguageOpen] = useState(false);
  const [collapsed, setCollapsed] = useState(() => localStorage.getItem('mis.hr.sidebar') === 'collapsed');
  const { isRtl, language, setLanguage, t } = useLocalization(); const { logout, user } = useAuth(); const navigate = useNavigate();
  const toggleCollapsed = () => setCollapsed(value => { const next = !value; localStorage.setItem('mis.hr.sidebar', next ? 'collapsed' : 'expanded'); return next; });
  const signOut = () => { logout(); navigate('/login', { replace: true }); }; const profileLabel = language === 'ar' ? 'ملفي الشخصي' : 'My profile';
  return <div className="module-shell min-h-screen bg-mis-surface text-mis-ink" data-sidebar-collapsed={collapsed}>
    {open && <button aria-label={t('closeNavigation')} className="fixed inset-0 z-30 bg-mis-ink/40 lg:hidden" onClick={() => setOpen(false)} type="button" />}
    <aside style={{ insetInlineStart: 0, borderInlineEndWidth: 1 }} className={`fixed inset-y-0 z-40 flex w-72 flex-col border-mis-border bg-white shadow-sm transition-[width,transform] duration-200 lg:translate-x-0 ${collapsed ? 'lg:w-20' : 'lg:w-72'} ${open ? 'translate-x-0' : isRtl ? 'translate-x-full' : '-translate-x-full'}`}>
      <div className={`flex h-24 items-center border-b border-mis-border ${collapsed ? 'lg:justify-center lg:px-2' : 'justify-between px-6'}`}><img src={misLogo} alt={t('companyLogoAlt')} className={`${collapsed ? 'lg:h-10' : 'h-16'} w-auto`} /><div className="flex"><button aria-label={collapsed ? 'Expand navigation' : 'Collapse navigation'} title={collapsed ? 'Expand navigation' : 'Collapse navigation'} className="hidden rounded-lg p-2 text-slate-500 hover:bg-slate-100 lg:inline-flex" onClick={toggleCollapsed}>{collapsed ? <PanelLeftOpen className="h-5 w-5" /> : <PanelLeftClose className="h-5 w-5" />}</button><button aria-label={t('closeNavigation')} className="rounded-lg p-2 text-slate-500 lg:hidden" onClick={() => setOpen(false)}><X /></button></div></div>
      <div className={`pb-3 pt-6 ${collapsed ? 'lg:hidden' : 'px-6'}`}><p className="text-xs font-bold uppercase tracking-widest text-mis-primary">{t('humanResources')}</p><p className="mt-1 text-sm text-slate-500">{t('hrDepartment')}</p></div>
      <nav className={`min-h-0 flex-1 space-y-1 overflow-y-auto py-3 ${collapsed ? 'lg:px-2' : 'px-4'}`} aria-label={t('hrNavigation')}>
        {links.map(({ to, label, icon: Icon }) => <NavLink key={to} to={to} title={collapsed ? t(label as TranslationKey) : undefined} onClick={() => setOpen(false)} className={({ isActive }) => `flex min-h-12 items-center gap-3 rounded-xl px-4 text-sm font-semibold transition ${collapsed ? 'lg:justify-center lg:px-2' : ''} ${isActive ? 'bg-mis-pale text-mis-deep' : 'text-slate-600 hover:bg-slate-50 hover:text-mis-primary'}`}><Icon className="h-5 w-5 shrink-0" aria-hidden="true" /><span className={collapsed ? 'lg:hidden' : ''}>{t(label as TranslationKey)}</span></NavLink>)}
        <button title={t('language')} className={`flex min-h-12 w-full items-center gap-3 rounded-xl px-4 text-sm font-semibold text-slate-600 hover:bg-slate-50 hover:text-mis-primary ${collapsed ? 'lg:justify-center lg:px-2' : ''}`} onClick={() => collapsed ? setLanguage(language === 'ar' ? 'en' : 'ar') : setLanguageOpen(value => !value)} type="button"><Languages className="h-5 w-5 shrink-0" /><span className={`flex-1 text-start ${collapsed ? 'lg:hidden' : ''}`}>{t('language')}</span><span className={`text-xs text-slate-400 ${collapsed ? 'lg:hidden' : ''}`}>{language === 'ar' ? 'العربية' : 'EN'}</span></button>
        {languageOpen && !collapsed && <div className="ms-4 space-y-1 border-s border-mis-border ps-3"><button className={`block w-full rounded-lg px-3 py-2 text-start text-sm ${language === 'en' ? 'bg-mis-pale font-semibold text-mis-deep' : 'text-slate-600'}`} onClick={() => setLanguage('en')}>{t('english')}</button><button className={`block w-full rounded-lg px-3 py-2 text-start text-sm ${language === 'ar' ? 'bg-mis-pale font-semibold text-mis-deep' : 'text-slate-600'}`} onClick={() => setLanguage('ar')}>{t('arabic')}</button></div>}
      </nav>
      <div className={`border-t border-mis-border ${collapsed ? 'lg:p-2' : 'p-4'}`}><NavLink to="/hr/profile" title={profileLabel} className={({ isActive }) => `mb-2 flex items-center gap-3 rounded-xl px-3 py-2.5 hover:bg-slate-50 ${collapsed ? 'lg:justify-center lg:px-2' : ''} ${isActive ? 'bg-mis-pale text-mis-deep' : 'text-slate-600'}`}><UserCircle className="h-5 w-5 shrink-0" /><div className={`min-w-0 ${collapsed ? 'lg:hidden' : ''}`}><p className="truncate text-sm font-semibold text-mis-navy">{user?.fullName}</p><p className="truncate text-xs text-slate-500">{profileLabel}</p></div></NavLink><button title={t('logout')} className={`flex w-full items-center gap-3 rounded-xl px-3 py-3 text-sm font-semibold text-slate-600 hover:bg-slate-50 ${collapsed ? 'lg:justify-center lg:px-2' : ''}`} onClick={signOut} type="button"><LogOut className="h-5 w-5 shrink-0" /><span className={collapsed ? 'lg:hidden' : ''}>{t('logout')}</span></button></div>
    </aside>
    <div className="module-content"><header className="sticky top-0 z-20 flex h-20 items-center border-b border-mis-border bg-white/95 px-5 backdrop-blur sm:px-8"><button aria-label={t('openNavigation')} className="me-4 rounded-lg border border-mis-border p-2 text-mis-navy lg:hidden" onClick={() => setOpen(true)} type="button"><Menu /></button><div><p className="text-xs font-semibold uppercase tracking-wide text-mis-primary">{t('collectionFirm')}</p><p className="mt-1 font-bold text-mis-navy">{t('humanResources')}</p></div></header><main className="p-5 sm:p-8"><Outlet /></main></div>
  </div>;
}
