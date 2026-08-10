import { Activity, ArrowRight, CalendarX2, FileWarning, FolderOpen, RefreshCw, UsersRound, UserRoundCheck } from 'lucide-react';
import { useCallback, useEffect, useState, type ReactNode } from 'react';
import { Link } from 'react-router-dom';
import { LoadingSpinner } from '../../components/common/LoadingSpinner';
import { useLocalization } from '../../context/LocalizationContext';
import { hrDashboardService } from '../../features/hr/services/hrDashboardService';
import type { HrDashboardSummary } from '../../features/hr/types/dashboard';
import { getApiErrorMessage } from '../../services/apiClient';

function Card({ label, value, context, icon }: { label: string; value: string | number; context: string; icon: ReactNode }) {
  return <article className="rounded-2xl border border-mis-border bg-white p-5 shadow-sm"><div className="flex items-start justify-between gap-4"><div><p className="text-sm font-semibold text-slate-500">{label}</p><p className="mt-3 text-3xl font-bold text-mis-navy">{value}</p></div><div className="flex h-11 w-11 items-center justify-center rounded-xl bg-mis-pale text-mis-primary">{icon}</div></div><p className="mt-4 text-xs text-slate-500">{context}</p></article>;
}
function Empty({ icon, title, description }: { icon: ReactNode; title: string; description: string }) {
  return <div className="flex min-h-48 items-center justify-center p-6 text-center"><div className="max-w-sm"><div className="mx-auto flex h-11 w-11 items-center justify-center rounded-xl bg-mis-pale text-mis-primary">{icon}</div><p className="mt-4 text-sm font-bold text-mis-navy">{title}</p><p className="mt-2 text-sm leading-6 text-slate-500">{description}</p></div></div>;
}
function Section({ title, children, action }: { title: string; children: ReactNode; action?: ReactNode }) {
  return <section className="rounded-2xl border border-mis-border bg-white shadow-sm"><header className="flex min-h-16 items-center justify-between gap-4 border-b border-mis-border px-5 py-4 sm:px-6"><h2 className="font-bold text-mis-navy">{title}</h2>{action}</header>{children}</section>;
}

export function HrDashboardPage() {
  const { t } = useLocalization();
  const [summary, setSummary] = useState<HrDashboardSummary | null>(null);
  const [error, setError] = useState('');
  const [loading, setLoading] = useState(true);
  const load = useCallback(async () => { setLoading(true); setError(''); try { setSummary(await hrDashboardService.getSummary()); } catch (requestError) { setError(getApiErrorMessage(requestError, t('loadDashboardError'))); } finally { setLoading(false); } }, [t]);
  useEffect(() => { void load(); }, [load]);
  if (loading) return <div className="flex min-h-[420px] items-center justify-center"><LoadingSpinner /></div>;
  if (!summary) return <div className="flex min-h-[420px] items-center justify-center rounded-2xl border border-mis-border bg-white p-8 text-center"><div><p className="font-bold text-mis-navy">{t('dashboardUnavailable')}</p><p className="mt-2 text-sm text-slate-500">{error}</p><button className="mt-5 inline-flex items-center gap-2 rounded-xl bg-mis-primary px-4 py-3 text-sm font-semibold text-white" onClick={() => void load()}><RefreshCw className="h-4 w-4" />{t('tryAgain')}</button></div></div>;
  const departments = summary.employeesByDepartment.filter((item) => item.employeeCount > 0);
  const maximum = Math.max(...departments.map((item) => item.employeeCount), 1);
  return <div className="mx-auto max-w-7xl"><div className="mb-7"><p className="text-sm font-semibold text-mis-primary">{t('hrDepartment')}</p><h1 className="mt-2 text-3xl font-bold text-mis-navy">{t('hrDashboard')}</h1><p className="mt-2 text-sm text-slate-500">{t('dashboardSubtitle')}</p></div>
    <div className="grid gap-4 sm:grid-cols-2 xl:grid-cols-4"><Card label={t('totalEmployees')} value={summary.totalEmployees} context={t('allEmployeeRecords')} icon={<UsersRound className="h-5 w-5" />} /><Card label={t('activeEmployees')} value={summary.activeEmployees} context={summary.totalEmployees ? t('totalEmployeePercent', { value: Math.round(summary.activeEmployees / summary.totalEmployees * 100) }) : t('noEmployeeRecords')} icon={<UserRoundCheck className="h-5 w-5" />} /><Card label={t('absentToday')} value={summary.attendanceAvailable ? summary.absentToday ?? 0 : '—'} context={summary.attendanceAvailable ? t('recordedToday') : t('notAvailableYet')} icon={<CalendarX2 className="h-5 w-5" />} /><Card label={t('documentsAttention')} value={summary.documentAttentionAvailable ? summary.documentsRequiringAttention ?? 0 : '—'} context={summary.documentAttentionAvailable ? t('totalDocumentsContext', { count: summary.totalDocuments }) : t('documentRecordsContext', { count: summary.totalDocuments })} icon={<FileWarning className="h-5 w-5" />}/></div>
    <div className="mt-6 grid gap-6 xl:grid-cols-3"><div className="xl:col-span-2"><Section title={t('employeeOverview')}>{departments.length ? <div className="space-y-5 p-6">{departments.map((item) => <div key={item.departmentId}><div className="mb-2 flex justify-between text-sm"><span className="font-semibold">{item.departmentName}</span><span className="font-bold text-mis-navy">{item.employeeCount}</span></div><div className="h-2 rounded-full bg-mis-pale"><div className="h-full rounded-full bg-mis-primary" style={{ width: `${Math.max(item.employeeCount / maximum * 100, 4)}%` }}/></div></div>)}</div> : <Empty icon={<UsersRound className="h-5 w-5" />} title={t('noEmployeeData')} description={t('employeeDataHelp')}/>}</Section></div><Section title={t('quickActions')}><div className="p-5"><Link className="flex items-center justify-between rounded-xl border border-mis-border p-4 text-sm font-semibold text-mis-navy hover:bg-mis-pale/50" to="/hr/employee-documents"><span className="flex items-center gap-3"><FolderOpen className="h-5 w-5 text-mis-primary"/>{t('viewEmployeeDocuments')}</span><ArrowRight className="h-4 w-4"/></Link></div></Section></div>
    <div className="mt-6 grid gap-6 xl:grid-cols-2"><Section title={t('attendanceOverview')}><Empty icon={<CalendarX2 className="h-5 w-5"/>} title={t('attendanceUnavailable')} description={t('attendanceHelp')}/></Section><Section title={t('recentActivity')}><Empty icon={<Activity className="h-5 w-5"/>} title={t('noRecentActivity')} description={t('noActivitySource')}/></Section></div>
    <div className="mt-6"><Section title={t('documentsAttention')} action={<Link className="text-sm font-semibold text-mis-primary" to="/hr/employee-documents">{t('viewDocuments')}</Link>}><Empty icon={<FileWarning className="h-5 w-5"/>} title={t('noDocumentsAttention')} description={t('documentsAttentionHelp')}/></Section></div>
  </div>;
}
