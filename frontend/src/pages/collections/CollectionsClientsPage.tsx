import { ChevronRight, Search } from 'lucide-react';
import { useEffect, useState } from 'react';
import { Link } from 'react-router-dom';
import { ErrorState } from '../../components/common/ErrorState';
import { LoadingSpinner } from '../../components/common/LoadingSpinner';
import { PageHeader } from '../../components/common/PageHeader';
import { Pagination } from '../../components/common/Pagination';
import { BankLogo } from '../../features/collections/components/BankLogo';
import { CollectionStatus, useCollectionFormat } from '../../features/collections/components/CollectionsUi';
import { useCollectionsLocalization } from '../../features/collections/localization/collectionsTranslations';
import { collectionsService } from '../../features/collections/services/collectionsService';
import type { ClientCard, PagedResult } from '../../features/collections/types/collections';

export function CollectionsClientsPage() {
  const { ct } = useCollectionsLocalization(); const f = useCollectionFormat(); const [search, setSearch] = useState(''); const [query, setQuery] = useState(''); const [page, setPage] = useState(1); const [data, setData] = useState<PagedResult<ClientCard>>(); const [error, setError] = useState(false);
  useEffect(() => { const timer = window.setTimeout(() => { setQuery(search); setPage(1); }, 350); return () => clearTimeout(timer); }, [search]);
  useEffect(() => { let active = true; setError(false); void collectionsService.clients({ page, pageSize: 12, search: query }).then(value => active && setData(value)).catch(() => active && setError(true)); return () => { active = false; }; }, [page, query]);
  return <div><PageHeader eyebrow={ct('collections')} title={ct('clients')} description={ct('clientCenterSubtitle')} />
    <div className="mb-6 max-w-xl"><label className="relative block"><Search className="absolute top-3.5 h-5 w-5 text-slate-400" style={{ insetInlineStart: '0.9rem' }} /><input className="w-full rounded-xl border border-mis-border bg-white py-3 pe-4 ps-12 outline-none focus:border-mis-blue focus:ring-4 focus:ring-mis-pale" value={search} onChange={event => setSearch(event.target.value)} placeholder={ct('searchClients')} /></label></div>
    {error ? <ErrorState title={ct('loadError')} onRetry={() => setQuery(value => `${value} `)} /> : !data ? <div className="flex min-h-72 items-center justify-center"><LoadingSpinner /></div> : data.items.length === 0 ? <div className="rounded-2xl border border-dashed border-mis-border bg-white p-12 text-center text-slate-500">{ct('noClients')}</div> : <><section className="grid gap-5 md:grid-cols-2 2xl:grid-cols-3">{data.items.map(client => <Link key={client.id} to={`/collections/clients/${client.id}`} className="group overflow-hidden rounded-3xl border border-mis-border bg-white shadow-sm transition hover:-translate-y-1 hover:border-mis-sky hover:shadow-panel"><div className="h-1.5 bg-gradient-to-r from-mis-primary via-mis-sky to-cyan-300" /><div className="p-6"><div className="flex items-start gap-4"><BankLogo code={client.code} name={client.name} logoUrl={client.logoUrl} /><div className="min-w-0 flex-1 pt-1"><p className="truncate text-lg font-bold text-mis-navy">{client.name}</p><p className="mt-1 text-xs font-semibold text-slate-400" data-bidi="ltr">{client.code} · {client.organizationType}</p></div><ChevronRight className="h-5 w-5 text-slate-300 transition group-hover:text-mis-primary rtl:rotate-180" /></div><div className="mt-6 grid grid-cols-2 gap-4 rounded-2xl bg-slate-50 p-4 text-sm"><Metric label={ct('portfolios')} value={f.number(client.activePortfolios)} /><Metric label={ct('casesCount')} value={f.number(client.totalCases)} /><Metric label={ct('totalOutstanding')} value={f.money(client.totalOutstanding)} /><Metric label={ct('achievement')} value={`${client.achievementPercent.toFixed(1)}%`} /></div><div className="mt-5 flex items-center justify-between"><span className="text-xs text-slate-500">{ct('health')}</span><CollectionStatus value={client.health} /></div></div></Link>)}</section><Pagination className="mt-6 rounded-2xl border border-mis-border bg-white" page={data.page} pageSize={data.pageSize} totalCount={data.totalCount} totalPages={data.totalPages} onPageChange={setPage} /></>}
  </div>;
}
function Metric({ label, value }: { label: string; value: string }) { return <div><p className="text-xs text-slate-500">{label}</p><p className="mt-1 truncate font-bold text-mis-navy" data-bidi="ltr">{value}</p></div>; }
