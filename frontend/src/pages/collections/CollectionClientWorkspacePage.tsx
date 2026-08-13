import { ArrowLeft } from 'lucide-react';
import { useEffect, useState } from 'react';
import { Link, useParams } from 'react-router-dom';
import { ErrorState } from '../../components/common/ErrorState';
import { LoadingSpinner } from '../../components/common/LoadingSpinner';
import { PageHeader } from '../../components/common/PageHeader';
import { KpiCard, useCollectionFormat } from '../../features/collections/components/CollectionsUi';
import { useCollectionsLocalization } from '../../features/collections/localization/collectionsTranslations';
import { collectionsService } from '../../features/collections/services/collectionsService';
import type { ClientCard, CollectionDashboard } from '../../features/collections/types/collections';

export function CollectionClientWorkspacePage() {
  const { id = '' } = useParams(); const { ct } = useCollectionsLocalization(); const f = useCollectionFormat(); const [client, setClient] = useState<ClientCard>(); const [data, setData] = useState<CollectionDashboard>(); const [error, setError] = useState(false);
  useEffect(() => { Promise.all([collectionsService.clients({ pageSize: 100 }), collectionsService.dashboard(id)]).then(([clients, dashboard]) => { setClient(clients.items.find(x => x.id === id)); setData(dashboard); }).catch(() => setError(true)); }, [id]);
  if (error) return <ErrorState title={ct('loadError')} />; if (!client || !data) return <div className="flex min-h-72 items-center justify-center"><LoadingSpinner /></div>;
  return <div><PageHeader breadcrumbs={<Link to="/collections/clients" className="inline-flex items-center gap-2 text-sm font-semibold text-mis-primary"><ArrowLeft className="h-4 w-4 rtl:rotate-180" />{ct('clients')}</Link>} eyebrow={client.code} title={client.name} description={ct('dashboardSubtitle')} actions={<Link to={`/collections/cases?organizationId=${id}`} className="rounded-xl bg-mis-primary px-4 py-2.5 text-sm font-bold text-white">{ct('cases')}</Link>} />
    <section className="grid gap-4 sm:grid-cols-2 xl:grid-cols-4"><KpiCard label={ct('totalCases')} value={f.number(data.totalCases)} /><KpiCard label={ct('totalOutstanding')} value={f.money(data.totalOutstanding)} /><KpiCard label={ct('totalOverdue')} value={f.money(data.totalOverdue)} accent="red" /><KpiCard label={ct('achievement')} value={`${data.achievementPercent.toFixed(1)}%`} accent="green" /><KpiCard label={ct('assigned')} value={f.number(data.assignedCases)} /><KpiCard label={ct('unassigned')} value={f.number(data.unassignedCases)} accent="amber" /><KpiCard label={ct('collectedToday')} value={f.money(data.collectedToday)} accent="green" /><KpiCard label={ct('collectedMtd')} value={f.money(data.collectedMonthToDate)} accent="green" /><KpiCard label={ct('activePtp')} value={f.number(data.activePromises)} /><KpiCard label={ct('dueToday')} value={f.number(data.promisesDueToday)} accent="amber" /><KpiCard label={ct('brokenPtp')} value={f.number(data.brokenPromises)} accent="red" /><KpiCard label={ct('pendingReviews')} value={f.number(data.pendingReviews)} accent="amber" /></section>
  </div>;
}
