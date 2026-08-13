import {
  Activity,
  AlertTriangle,
  BriefcaseBusiness,
  CalendarCheck2,
  CalendarClock,
  CalendarDays,
  CalendarX2,
  FileWarning,
  RefreshCw,
  TimerOff,
  UserRoundCheck,
  UserRoundMinus,
  UsersRound,
} from 'lucide-react';
import { useCallback, useEffect, useMemo, useState, type ReactNode } from 'react';
import { Link } from 'react-router-dom';
import { Button } from '../../components/common/Button';
import { Card } from '../../components/common/Card';
import { EmptyState } from '../../components/common/EmptyState';
import { ErrorState } from '../../components/common/ErrorState';
import { LoadingSpinner } from '../../components/common/LoadingSpinner';
import { PageHeader } from '../../components/common/PageHeader';
import { Section } from '../../components/common/Section';
import { StatusBadge, type StatusTone } from '../../components/common/StatusBadge';
import { useLocalization } from '../../context/LocalizationContext';
import { hrDashboardService } from '../../features/hr/services/hrDashboardService';
import type {
  AttendanceTrendPoint,
  DepartmentEmployeeCount,
  HrDashboardSummary,
} from '../../features/hr/types/dashboard';
import { getApiErrorMessage } from '../../services/apiClient';

const dashboardCopy = {
  en: {
    inactiveEmployees: 'Inactive Employees', todayAttendance: "Today's Attendance", present: 'Present', absent: 'Absent', late: 'Late', onLeave: 'On Leave', missingCheckOut: 'Missing Check-Out',
    byDepartment: 'Employees by Department', alerts: 'HR Alerts', noAlerts: 'No current alerts', noAlertsHelp: 'Expiring contracts, documents, probation dates, and birthdays will appear here.',
    attendanceTrend: 'Attendance Trend', absenceTrend: 'Absence Trend', lastThirtyDays: 'Last 30 days from processed attendance records', noTrend: 'No processed data for this period',
    activity: 'Recent HR Activity', noActivity: 'No recent HR activity', noActivityHelp: 'Important HR changes will appear after they are recorded.',
    daysRemaining: '{days} days remaining', overdue: '{days} days overdue', dueToday: 'Due today', reports: 'Open Reports', importAttendance: 'Import Attendance', refreshed: 'Live data from the HR API',
    unassigned: 'Unassigned', people: 'employees', refresh: 'Refresh', viewEmployee: 'View employee', retry: 'Try again', unavailable: 'Dashboard unavailable',
  },
  ar: {
    inactiveEmployees: 'الموظفون غير النشطين', todayAttendance: 'حضور اليوم', present: 'حاضر', absent: 'غائب', late: 'متأخر', onLeave: 'في إجازة', missingCheckOut: 'بدون تسجيل خروج',
    byDepartment: 'الموظفون حسب القسم', alerts: 'تنبيهات الموارد البشرية', noAlerts: 'لا توجد تنبيهات حالية', noAlertsHelp: 'ستظهر هنا العقود والمستندات وفترات الاختبار القريبة من الانتهاء وأعياد الميلاد.',
    attendanceTrend: 'اتجاه الحضور', absenceTrend: 'اتجاه الغياب', lastThirtyDays: 'آخر 30 يومًا من سجلات الحضور المعالجة', noTrend: 'لا توجد بيانات معالجة لهذه الفترة',
    activity: 'أحدث نشاطات الموارد البشرية', noActivity: 'لا يوجد نشاط حديث', noActivityHelp: 'ستظهر التغييرات المهمة بعد تسجيلها في النظام.',
    daysRemaining: 'متبقي {days} يوم', overdue: 'متأخر {days} يوم', dueToday: 'موعده اليوم', reports: 'فتح التقارير', importAttendance: 'استيراد الحضور', refreshed: 'بيانات مباشرة من واجهة الموارد البشرية',
    unassigned: 'غير محدد', people: 'موظف', refresh: 'تحديث', viewEmployee: 'عرض الموظف', retry: 'إعادة المحاولة', unavailable: 'لوحة التحكم غير متاحة',
  },
} as const;

function MetricCard({ context, icon, label, value }: { context: string; icon: ReactNode; label: string; value: number }) {
  return (
    <Card className="relative overflow-hidden" padding="md">
      <div className="absolute -end-5 -top-5 h-24 w-24 rounded-full bg-mis-pale/70" aria-hidden="true" />
      <div className="relative flex items-start justify-between gap-4">
        <div><p className="text-sm font-semibold text-slate-500">{label}</p><p className="mt-3 text-3xl font-bold tabular-nums text-mis-navy">{value}</p></div>
        <div className="flex h-11 w-11 items-center justify-center rounded-xl bg-mis-pale text-mis-primary">{icon}</div>
      </div>
      <p className="relative mt-4 text-xs text-slate-500">{context}</p>
    </Card>
  );
}

function DistributionBars({ emptyText, items }: { emptyText: string; items: DepartmentEmployeeCount[]; }) {
  const populated = items.filter((item) => item.employeeCount > 0);
  const maximum = Math.max(...populated.map((item) => item.employeeCount), 1);
  if (!populated.length) return <EmptyState compact description={emptyText} title={emptyText} />;

  return (
    <div className="space-y-4">
      {populated.map((item) => {
        return (
          <div key={item.departmentId}>
            <div className="mb-2 flex items-center justify-between gap-4 text-sm"><span className="truncate font-semibold text-slate-700">{item.departmentName}</span><span className="font-bold tabular-nums text-mis-navy">{item.employeeCount}</span></div>
            <div className="h-2 overflow-hidden rounded-full bg-mis-pale"><div className="h-full rounded-full bg-mis-primary transition-all" style={{ width: `${Math.max((item.employeeCount / maximum) * 100, 3)}%` }} /></div>
          </div>
        );
      })}
    </div>
  );
}

const trendColors = { present: 'bg-emerald-500', late: 'bg-amber-400', absent: 'bg-red-500', onLeave: 'bg-sky-500' } as const;

function AttendanceTrend({ copy, locale, points }: { copy: typeof dashboardCopy.en | typeof dashboardCopy.ar; locale: string; points: AttendanceTrendPoint[] }) {
  const visible = points.slice(-14);
  const maximum = Math.max(...visible.flatMap((point) => [point.present, point.late, point.absent, point.onLeave]), 1);
  if (!visible.some((point) => point.present || point.late || point.absent || point.onLeave)) return <EmptyState compact description={copy.lastThirtyDays} title={copy.noTrend} />;

  const legend = [
    ['present', copy.present], ['late', copy.late], ['absent', copy.absent], ['onLeave', copy.onLeave],
  ] as const;
  return (
    <div>
      <div className="flex h-48 items-end gap-1.5 border-b border-slate-200 pb-1" aria-label={copy.attendanceTrend} role="img">
        {visible.map((point) => (
          <div className="group flex h-full min-w-0 flex-1 items-end justify-center gap-px" key={point.date} title={`${new Intl.DateTimeFormat(locale, { dateStyle: 'medium' }).format(new Date(`${point.date}T00:00:00`))}: ${copy.present} ${point.present}, ${copy.late} ${point.late}, ${copy.absent} ${point.absent}, ${copy.onLeave} ${point.onLeave}`}>
            {legend.map(([key]) => <div className={`min-h-px w-1/4 max-w-2.5 rounded-t ${trendColors[key]}`} key={key} style={{ height: `${Math.max((point[key] / maximum) * 100, point[key] ? 3 : 0)}%` }} />)}
          </div>
        ))}
      </div>
      <div className="mt-2 flex justify-between text-[10px] text-slate-400"><span>{visible[0] ? new Intl.DateTimeFormat(locale, { day: 'numeric', month: 'short' }).format(new Date(`${visible[0].date}T00:00:00`)) : ''}</span><span>{visible.at(-1) ? new Intl.DateTimeFormat(locale, { day: 'numeric', month: 'short' }).format(new Date(`${visible.at(-1)!.date}T00:00:00`)) : ''}</span></div>
      <div className="mt-4 flex flex-wrap gap-4">{legend.map(([key, label]) => <span className="flex items-center gap-2 text-xs text-slate-600" key={key}><span className={`h-2.5 w-2.5 rounded-full ${trendColors[key]}`} />{label}</span>)}</div>
    </div>
  );
}

function AbsenceTrend({ copy, locale, points }: { copy: typeof dashboardCopy.en | typeof dashboardCopy.ar; locale: string; points: HrDashboardSummary['absenceTrend'] }) {
  const visible = points.slice(-14);
  const maximum = Math.max(...visible.map((point) => point.absences), 1);
  if (!visible.some((point) => point.absences)) return <EmptyState compact description={copy.lastThirtyDays} title={copy.noTrend} />;
  return (
    <div className="flex h-48 items-end gap-1.5 border-b border-slate-200 pb-1" aria-label={copy.absenceTrend} role="img">
      {visible.map((point) => <div className="min-w-0 flex-1 rounded-t bg-red-400" key={point.date} style={{ height: `${Math.max((point.absences / maximum) * 100, 3)}%` }} title={`${new Intl.DateTimeFormat(locale).format(new Date(`${point.date}T00:00:00`))}: ${point.absences}`} />)}
    </div>
  );
}

function alertTone(severity: string): StatusTone {
  if (severity.toLowerCase() === 'danger' || severity.toLowerCase() === 'critical') return 'danger';
  if (severity.toLowerCase() === 'warning') return 'warning';
  return 'info';
}

export function HrDashboardPage() {
  const { language, t } = useLocalization();
  const copy = dashboardCopy[language];
  const locale = language === 'ar' ? 'ar-EG' : 'en-GB';
  const [summary, setSummary] = useState<HrDashboardSummary | null>(null);
  const [error, setError] = useState('');
  const [loading, setLoading] = useState(true);

  const load = useCallback(async () => {
    setLoading(true);
    setError('');
    try { setSummary(await hrDashboardService.getSummary()); }
    catch (requestError) { setError(getApiErrorMessage(requestError, t('loadDashboardError'))); }
    finally { setLoading(false); }
  }, [t]);

  useEffect(() => { void load(); }, [load]);

  const attendanceCards = useMemo(() => summary ? [
    { label: copy.present, value: summary.todayAttendance.present, tone: 'bg-emerald-50 text-emerald-700', icon: <CalendarCheck2 className="h-5 w-5" /> },
    { label: copy.absent, value: summary.todayAttendance.absent, tone: 'bg-red-50 text-red-700', icon: <CalendarX2 className="h-5 w-5" /> },
    { label: copy.late, value: summary.todayAttendance.late, tone: 'bg-amber-50 text-amber-700', icon: <CalendarClock className="h-5 w-5" /> },
    { label: copy.onLeave, value: summary.todayAttendance.onLeave, tone: 'bg-sky-50 text-sky-700', icon: <CalendarDays className="h-5 w-5" /> },
    { label: copy.missingCheckOut, value: summary.todayAttendance.missingCheckOut, tone: 'bg-violet-50 text-violet-700', icon: <TimerOff className="h-5 w-5" /> },
  ] : [], [copy, summary]);

  if (loading && !summary) return <div className="flex min-h-[420px] items-center justify-center"><LoadingSpinner /></div>;
  if (!summary) return <ErrorState message={error} onRetry={() => void load()} retryLabel={copy.retry} title={copy.unavailable} />;

  return (
    <div className="mx-auto max-w-7xl">
      <PageHeader
        actions={<div className="flex flex-wrap gap-2"><Button fullWidth={false} leftIcon={<RefreshCw className="h-4 w-4" />} isLoading={loading} onClick={() => void load()} variant="outline">{copy.refresh}</Button><Link className="inline-flex h-10 items-center rounded-xl bg-mis-primary px-4 text-sm font-semibold text-white hover:bg-mis-deep" to="/hr/reports">{copy.reports}</Link></div>}
        description={copy.refreshed}
        eyebrow={t('hrDepartment')}
        title={t('hrDashboard')}
      />

      <div className="grid gap-4 sm:grid-cols-2 xl:grid-cols-4">
        <MetricCard context={t('allEmployeeRecords')} icon={<UsersRound className="h-5 w-5" />} label={t('totalEmployees')} value={summary.totalEmployees} />
        <MetricCard context={summary.totalEmployees ? t('totalEmployeePercent', { value: Math.round((summary.activeEmployees / summary.totalEmployees) * 100) }) : t('noEmployeeRecords')} icon={<UserRoundCheck className="h-5 w-5" />} label={t('activeEmployees')} value={summary.activeEmployees} />
        <MetricCard context={copy.refreshed} icon={<UserRoundMinus className="h-5 w-5" />} label={copy.inactiveEmployees} value={summary.inactiveEmployees} />
        <MetricCard context={t('totalDocumentsContext', { count: summary.totalDocuments })} icon={<FileWarning className="h-5 w-5" />} label={t('documentsAttention')} value={summary.documentsRequiringAttention ?? 0} />
      </div>

      <Section className="mt-6" description={copy.refreshed} title={copy.todayAttendance}>
        <div className="grid gap-3 sm:grid-cols-2 lg:grid-cols-5">
          {attendanceCards.map((item) => <div className={`rounded-xl p-4 ${item.tone}`} key={item.label}><div className="flex items-center justify-between gap-3"><span>{item.icon}</span><strong className="text-2xl tabular-nums">{item.value}</strong></div><p className="mt-3 text-sm font-semibold">{item.label}</p></div>)}
        </div>
      </Section>

      <Section className="mt-6" title={copy.byDepartment}><DistributionBars emptyText={t('employeeDataHelp')} items={summary.employeesByDepartment} /></Section>

      <div className="mt-6 grid gap-6 xl:grid-cols-[minmax(0,1.45fr)_minmax(320px,0.55fr)]">
        <Section description={copy.lastThirtyDays} title={copy.attendanceTrend}><AttendanceTrend copy={copy} locale={locale} points={summary.attendanceTrend} /></Section>
        <Section description={copy.lastThirtyDays} title={copy.absenceTrend}><AbsenceTrend copy={copy} locale={locale} points={summary.absenceTrend} /></Section>
      </div>

      <div className="mt-6 grid gap-6 xl:grid-cols-2">
        <Section action={<Link className="text-sm font-semibold text-mis-primary" to="/hr/employee-documents">{t('viewDocuments')}</Link>} bodyClassName="divide-y divide-mis-border" title={copy.alerts}>
          {summary.alerts.length ? summary.alerts.slice(0, 10).map((alert) => {
            const remaining = alert.daysRemaining === 0 ? copy.dueToday : alert.daysRemaining < 0 ? copy.overdue.replace('{days}', String(Math.abs(alert.daysRemaining))) : copy.daysRemaining.replace('{days}', String(alert.daysRemaining));
            return <article className="flex items-start gap-3 px-5 py-4" key={`${alert.category}-${alert.entityId}`}><div className="mt-0.5 rounded-lg bg-amber-50 p-2 text-amber-700"><AlertTriangle className="h-4 w-4" /></div><div className="min-w-0 flex-1"><div className="flex flex-wrap items-start justify-between gap-2"><p className="font-semibold text-mis-navy">{alert.title}</p><StatusBadge tone={alertTone(alert.severity)}>{remaining}</StatusBadge></div><p className="mt-1 text-sm text-slate-500">{alert.employeeNumber} · {alert.employeeName}</p><p className="mt-1 text-xs text-slate-400">{new Intl.DateTimeFormat(locale, { dateStyle: 'medium' }).format(new Date(`${alert.dueDate}T00:00:00`))}</p></div></article>;
          }) : <EmptyState compact description={copy.noAlertsHelp} icon={<FileWarning className="h-5 w-5" />} title={copy.noAlerts} />}
        </Section>

        <Section bodyClassName="divide-y divide-mis-border" title={copy.activity}>
          {summary.recentActivity.length ? summary.recentActivity.slice(0, 12).map((activity) => <article className="flex gap-3 px-5 py-4" key={activity.id}><div className="mt-0.5 rounded-lg bg-mis-pale p-2 text-mis-primary"><Activity className="h-4 w-4" /></div><div className="min-w-0 flex-1"><p className="font-semibold text-mis-navy">{activity.message}</p><p className="mt-1 text-sm text-slate-500">{activity.username}{activity.employeeName ? ` · ${activity.employeeName}` : ''}</p><time className="mt-1 block text-xs text-slate-400" dateTime={activity.timestamp}>{new Intl.DateTimeFormat(locale, { dateStyle: 'medium', timeStyle: 'short' }).format(new Date(activity.timestamp))}</time></div>{activity.employeeId ? <Link aria-label={copy.viewEmployee} className="text-mis-primary" to={`/hr/employees/${activity.employeeId}`}><BriefcaseBusiness className="h-4 w-4" /></Link> : null}</article>) : <EmptyState compact description={copy.noActivityHelp} icon={<Activity className="h-5 w-5" />} title={copy.noActivity} />}
        </Section>
      </div>

      <Card className="mt-6 flex flex-col items-start justify-between gap-4 sm:flex-row sm:items-center" padding="md">
        <div><p className="font-bold text-mis-navy">{copy.todayAttendance}</p><p className="mt-1 text-sm text-slate-500">{copy.lastThirtyDays}</p></div>
        <div className="flex flex-wrap gap-2"><Link className="inline-flex h-10 items-center gap-2 rounded-xl border border-mis-border px-4 text-sm font-semibold text-mis-navy hover:bg-mis-pale" to="/hr/attendance/import"><CalendarClock className="h-4 w-4" />{copy.importAttendance}</Link><Link className="inline-flex h-10 items-center gap-2 rounded-xl bg-mis-primary px-4 text-sm font-semibold text-white hover:bg-mis-deep" to="/hr/reports"><BriefcaseBusiness className="h-4 w-4" />{copy.reports}</Link></div>
      </Card>
    </div>
  );
}
