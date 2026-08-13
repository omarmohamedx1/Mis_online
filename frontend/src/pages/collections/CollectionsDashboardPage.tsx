import { AlertTriangle, ArrowUpRight, CalendarClock, PhoneCall } from 'lucide-react';
import { useEffect, useState } from 'react';
import { Link, useSearchParams } from 'react-router-dom';
import { ErrorState } from '../../components/common/ErrorState';
import { LoadingSpinner } from '../../components/common/LoadingSpinner';
import { PageHeader } from '../../components/common/PageHeader';
import { CollectionStatus, KpiCard, useCollectionFormat } from '../../features/collections/components/CollectionsUi';
import { useCollectionsLocalization } from '../../features/collections/localization/collectionsTranslations';
import { collectionsService } from '../../features/collections/services/collectionsService';
import type { CollectionDashboard, WorkQueue } from '../../features/collections/types/collections';

export function CollectionsDashboardPage() {
  const { ct } = useCollectionsLocalization(); const f = useCollectionFormat(); const [params] = useSearchParams(); const organizationId = params.get('organizationId') ?? undefined;
  const [data, setData] = useState<CollectionDashboard>(); const [work, setWork] = useState<WorkQueue>(); const [error, setError] = useState(false);
  useEffect(() => { let active = true; setError(false); Promise.all([collectionsService.dashboard(organizationId), collectionsService.myWork()]).then(([dashboard, queue]) => { if (active) { setData(dashboard); setWork(queue); } }).catch(() => active && setError(true)); return () => { active = false; }; }, [organizationId]);
  if (error) return <ErrorState title={ct('loadError')} onRetry={() => window.location.reload()} />; if (!data || !work) return <div className="flex min-h-[420px] items-center justify-center"><LoadingSpinner /></div>;
  const kpis = [
    [ct('totalCases'), f.number(data.totalCases), 'blue'], [ct('totalOutstanding'), f.money(data.totalOutstanding), 'blue'], [ct('totalOverdue'), f.money(data.totalOverdue), 'red'], [ct('achievement'), `${data.achievementPercent.toFixed(1)}%`, 'green'],
    [ct('collectedToday'), f.money(data.collectedToday), 'green'], [ct('collectedMtd'), f.money(data.collectedMonthToDate), 'green'], [ct('unassigned'), f.number(data.unassignedCases), data.unassignedCases ? 'amber' : 'blue'], [ct('pendingReviews'), f.number(data.pendingReviews), data.pendingReviews ? 'amber' : 'blue'],
    [ct('activePtp'), f.number(data.activePromises), 'blue'], [ct('dueToday'), f.number(data.promisesDueToday), 'amber'], [ct('brokenPtp'), f.number(data.brokenPromises), 'red'], [ct('highRisk'), f.number(data.highRiskCases), 'red'],
  ] as const;
  return <div><PageHeader eyebrow={ct('collections')} title={ct('commandCenter')} description={ct('dashboardSubtitle')} />
    <section className="grid gap-4 sm:grid-cols-2 xl:grid-cols-4">{kpis.map(([label, value, accent]) => <KpiCard key={label} label={label} value={value} accent={accent} />)}</section>
    <section className="mt-8"><div className="mb-4 flex items-center justify-between"><h2 className="text-xl font-bold text-mis-navy">{ct('myWork')}</h2><Link to="/collections/cases" className="inline-flex items-center gap-1 text-sm font-semibold text-mis-primary">{ct('cases')}<ArrowUpRight className="h-4 w-4 rtl:rotate-[-90deg]" /></Link></div>
      <div className="grid gap-5 xl:grid-cols-2"><Queue title={ct('callsDue')} icon={<PhoneCall className="h-5 w-5" />} rows={work.callsDue} empty={ct('noWork')} /><Queue title={ct('priorityCases')} icon={<AlertTriangle className="h-5 w-5" />} rows={work.highPriorityCases} empty={ct('noWork')} /></div>
      <div className="mt-5 grid gap-4 sm:grid-cols-3"><KpiCard label={ct('visitsToday')} value={f.number(work.visitsToday)} accent="blue" /><KpiCard label={ct('pendingReviews')} value={f.number(work.pendingReviews)} accent="amber" /><KpiCard label={ct('complaints')} value={f.number(work.openComplaints)} accent="red" /></div>
    </section>
  </div>;
}

function Queue({ title, icon, rows, empty }: { title: string; icon: React.ReactNode; rows: WorkQueue['callsDue']; empty: string }) {
  return <article className="overflow-hidden rounded-2xl border border-mis-border bg-white shadow-sm"><header className="flex items-center gap-2 border-b border-mis-border px-5 py-4 font-bold text-mis-navy">{icon}{title}</header>{rows.length ? <ul className="divide-y divide-mis-border">{rows.map(row => <li key={row.id} className="flex items-center gap-3 px-5 py-4"><div className="min-w-0 flex-1"><p className="truncate font-semibold text-mis-navy">{row.customerName}</p><p className="mt-1 text-xs text-slate-500"><span data-bidi="ltr">{row.caseNumber}</span> · {row.bucket}</p></div><CollectionStatus value={row.priority} /><Link to={`/collections/cases/${row.id}`} aria-label={row.caseNumber} className="rounded-lg p-2 text-mis-primary hover:bg-mis-pale"><CalendarClock className="h-4 w-4" /></Link></li>)}</ul> : <p className="p-6 text-sm text-slate-500">{empty}</p>}</article>;
}
