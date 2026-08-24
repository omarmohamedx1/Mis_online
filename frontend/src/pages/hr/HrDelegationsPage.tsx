import { Eye, FilePenLine, Plus, Printer, Search, ShieldX } from 'lucide-react';
import { useCallback, useEffect, useState, type FormEvent } from 'react';
import { Button } from '../../components/common/Button';
import { Card } from '../../components/common/Card';
import { EmptyState } from '../../components/common/EmptyState';
import { ErrorState } from '../../components/common/ErrorState';
import { LoadingSpinner } from '../../components/common/LoadingSpinner';
import { Modal } from '../../components/common/Modal';
import { PageHeader } from '../../components/common/PageHeader';
import { Pagination } from '../../components/common/Pagination';
import { StatusBadge } from '../../components/common/StatusBadge';
import { useToast } from '../../components/common/Toast';
import { EmployeeSearchSelect } from '../../features/hr/components/EmployeeSearchSelect';
import { DelegationDocument } from '../../features/hr/components/DelegationDocument';
import { hrDelegationService } from '../../features/hr/services/hrDelegationService';
import { hrEmployeeProfileService } from '../../features/hr/services/hrEmployeeProfileService';
import type { DelegationDetails, DelegationEntityOption, DelegationListItem, DelegationQuery, PagedDelegations, SaveDelegationRequest } from '../../features/hr/types/delegation';
import type { EmployeeListItem } from '../../features/hr/types/employee';
import { getApiErrorMessage } from '../../services/apiClient';

const officialAuthorization = 'وذلك للتفاوض مع عملائه وأيضاً بالتعامل مع جهات تنفيذ الأحكام الجنائية والمدنية بأقسام الشرطة بجمهورية مصر العربية ومديريات الأمن.';
const emptyPage: PagedDelegations = { items: [], page: 1, pageSize: 20, totalCount: 0, totalPages: 0 };
const initialQuery: DelegationQuery = { page: 1, pageSize: 20, search: '', employeeId: '', departmentId: '', delegationTypeId: '', delegatingEntityId: '', status: '', dateFrom: '', dateTo: '', sortBy: 'createdAt', sortDirection: 'desc' };
const fieldClass = 'h-11 w-full rounded-xl border border-mis-border bg-white px-3 text-sm outline-none focus:border-mis-primary focus:ring-2 focus:ring-mis-primary/10';

function statusTone(value: string): 'success' | 'warning' | 'danger' | 'neutral' { return value === 'Active' ? 'success' : value === 'Cancelled' ? 'danger' : value === 'Draft' ? 'warning' : 'neutral'; }
function displayDate(value: string) { return new Intl.DateTimeFormat(undefined, { dateStyle: 'medium' }).format(new Date(`${value}T00:00:00`)); }
interface FormState extends SaveDelegationRequest { employeeId: string; }
const blankForm = (): FormState => ({ employeeId: '', delegatingEntityId: null, authorizedEntity: '', companyRepresentative: '', powerOfAttorneyNumber: '', powerOfAttorneyYear: new Date().getFullYear(), startDate: '', endDate: '', purpose: officialAuthorization, notes: '', status: 'Active' });

function DelegationForm({ item, entities, onClose, onSaved }: { item: DelegationDetails | null; entities: DelegationEntityOption[]; onClose: () => void; onSaved: (value: DelegationDetails) => void }) {
  const toast = useToast();
  const [form, setForm] = useState<FormState>(() => item ? { employeeId: item.employeeId, delegationTypeId: item.delegationTypeId, subject: item.subject, delegatingEntityId: item.delegatingEntityId, authorizedEntity: item.authorizedEntity ?? '', companyRepresentative: item.companyRepresentative, powerOfAttorneyNumber: item.powerOfAttorneyNumber, powerOfAttorneyYear: item.powerOfAttorneyYear, startDate: item.startDate, endDate: item.endDate, purpose: item.purpose, notes: item.notes, status: item.status === 'Draft' ? 'Draft' : 'Active' } : blankForm());
  const [employee, setEmployee] = useState<EmployeeListItem | null>(item ? { id: item.employeeId, employeeNumber: item.employeeNumber, fullName: item.employeeName, departmentId: '', departmentName: item.departmentName, departmentCode: '', isActive: true } : null);
  const [nationalId, setNationalId] = useState(item?.employeeNationalId ?? '');
  const [saving, setSaving] = useState(false);
  const [error, setError] = useState('');
  const set = <K extends keyof FormState>(key: K, value: FormState[K]) => setForm(current => ({ ...current, [key]: value }));

  async function selectEmployee(id: string, selected: EmployeeListItem | null) {
    set('employeeId', id); setEmployee(selected); setNationalId('');
    if (id) try { const profile = await hrEmployeeProfileService.getProfile(id); setNationalId(profile.personal.nationalId ?? ''); } catch { setNationalId(''); }
  }
  function selectEntity(id: string) {
    const entity = entities.find(option => option.id === id);
    setForm(current => ({ ...current, delegatingEntityId: id || null, authorizedEntity: entity?.nameArabic ?? '' }));
  }
  async function submit(event: FormEvent) {
    event.preventDefault(); setError('');
    if (!form.employeeId || !form.authorizedEntity.trim() || !form.startDate || !form.endDate || !form.purpose.trim()) { setError('Complete all required fields.'); return; }
    if (form.endDate < form.startDate) { setError('End Date must be on or after Start Date.'); return; }
    if (!nationalId) { setError('The selected employee has no National ID. Update the employee profile first.'); return; }
    setSaving(true);
    try {
      const payload: SaveDelegationRequest = { ...form, employeeId: item ? undefined : form.employeeId, authorizedEntity: form.authorizedEntity.trim(), companyRepresentative: form.companyRepresentative?.trim() || null, powerOfAttorneyNumber: form.powerOfAttorneyNumber?.trim() || null, notes: form.notes?.trim() || null };
      const saved = item ? await hrDelegationService.update(item.id, payload) : await hrDelegationService.create(payload);
      toast.success(item ? 'Delegation updated.' : 'Delegation created.'); onSaved(saved);
    } catch (reason) { setError(getApiErrorMessage(reason, 'Unable to save delegation.')); } finally { setSaving(false); }
  }
  return <Modal bodyClassName="p-0" closeOnBackdrop={!saving} footer={null} onClose={onClose} open size="xl" title={item ? 'Edit Delegation' : 'Create Delegation'}>
    <form className="p-6" onSubmit={submit}>
      <div className="grid gap-4 md:grid-cols-2">
        <div className="md:col-span-2"><EmployeeSearchSelect disabled={Boolean(item)} initialSelection={employee} label="Employee" onChange={(id, selected) => void selectEmployee(id, selected)} required value={form.employeeId} />
          {employee ? <div className="mt-2 grid gap-2 rounded-xl bg-slate-50 p-3 text-sm sm:grid-cols-3"><span><b>Name:</b> {employee.fullName}</span><span><b>Employee ID:</b> {employee.employeeNumber}</span><span><b>National ID:</b> {nationalId || 'Not recorded'}</span></div> : null}
          {item ? <p className="mt-2 text-xs text-amber-700">Employee identity is locked for historical/legal accuracy.</p> : null}
        </div>
        <label className="text-sm font-semibold text-slate-700">Bank / Delegating Entity *<select className={`${fieldClass} mt-2`} onChange={event => selectEntity(event.target.value)} value={form.delegatingEntityId ?? ''}><option value="">Enter another entity</option>{entities.map(entity => <option key={entity.id} value={entity.id}>{entity.nameArabic} — {entity.nameEnglish}</option>)}</select></label>
        <label className="text-sm font-semibold text-slate-700">Entity name *<input className={`${fieldClass} mt-2`} maxLength={250} onChange={event => { set('authorizedEntity', event.target.value); set('delegatingEntityId', null); }} required value={form.authorizedEntity} /></label>
        <label className="text-sm font-semibold text-slate-700">Company Representative<input className={`${fieldClass} mt-2`} maxLength={250} onChange={event => set('companyRepresentative', event.target.value)} value={form.companyRepresentative ?? ''} /></label>
        <div className="grid grid-cols-2 gap-3"><label className="text-sm font-semibold text-slate-700">POA Number<input className={`${fieldClass} mt-2`} maxLength={100} onChange={event => set('powerOfAttorneyNumber', event.target.value)} value={form.powerOfAttorneyNumber ?? ''} /></label><label className="text-sm font-semibold text-slate-700">POA Year<input className={`${fieldClass} mt-2`} max="9999" min="1900" onChange={event => set('powerOfAttorneyYear', event.target.value ? Number(event.target.value) : null)} type="number" value={form.powerOfAttorneyYear ?? ''} /></label></div>
        <label className="text-sm font-semibold text-slate-700">Start Date *<input className={`${fieldClass} mt-2`} onChange={event => set('startDate', event.target.value)} required type="date" value={form.startDate} /></label>
        <label className="text-sm font-semibold text-slate-700">End Date *<input className={`${fieldClass} mt-2`} min={form.startDate || undefined} onChange={event => set('endDate', event.target.value)} required type="date" value={form.endDate} /></label>
        <label className="text-sm font-semibold text-slate-700 md:col-span-2">Delegation Purpose / Authorization Text *<textarea className="mt-2 min-h-32 w-full rounded-xl border border-mis-border p-3 text-sm leading-7 outline-none focus:border-mis-primary" dir="rtl" maxLength={4000} onChange={event => set('purpose', event.target.value)} required value={form.purpose} /></label>
        <label className="text-sm font-semibold text-slate-700 md:col-span-2">Notes <span className="font-normal text-slate-400">(internal — not printed)</span><textarea className="mt-2 min-h-20 w-full rounded-xl border border-mis-border p-3 text-sm outline-none focus:border-mis-primary" maxLength={2000} onChange={event => set('notes', event.target.value)} value={form.notes ?? ''} /></label>
      </div>
      {error ? <p className="mt-4 rounded-xl bg-red-50 p-3 text-sm font-semibold text-red-700">{error}</p> : null}
      <div className="mt-6 flex justify-end gap-3"><Button fullWidth={false} onClick={onClose} type="button" variant="outline">Close</Button><Button fullWidth={false} isLoading={saving} type="submit">{item ? 'Save Changes' : 'Save & View Delegation'}</Button></div>
    </form>
  </Modal>;
}

function Preview({ item, onClose, onEdit }: { item: DelegationDetails; onClose: () => void; onEdit: () => void }) {
  return <Modal bodyClassName="overflow-auto bg-slate-200 p-5" footer={<><Button fullWidth={false} leftIcon={<FilePenLine className="h-4 w-4" />} onClick={onEdit} variant="outline">Edit</Button><Button fullWidth={false} leftIcon={<Printer className="h-4 w-4" />} onClick={() => window.print()}>طباعة التفويض</Button><Button fullWidth={false} onClick={onClose} variant="outline">Close</Button></>} onClose={onClose} open size="full" title={`Delegation Preview · ${item.delegationNumber}`}>
    <div className="delegation-print-root"><DelegationDocument delegation={item} /></div>
  </Modal>;
}

export function HrDelegationsPage() {
  const toast = useToast();
  const [query, setQuery] = useState(initialQuery); const [data, setData] = useState(emptyPage); const [entities, setEntities] = useState<DelegationEntityOption[]>([]);
  const [loading, setLoading] = useState(true); const [error, setError] = useState(''); const [formOpen, setFormOpen] = useState(false); const [editing, setEditing] = useState<DelegationDetails | null>(null); const [preview, setPreview] = useState<DelegationDetails | null>(null); const [cancelTarget, setCancelTarget] = useState<DelegationListItem | null>(null); const [cancelReason, setCancelReason] = useState('');
  const load = useCallback(async () => { setLoading(true); setError(''); try { setData(await hrDelegationService.getPaged(query)); } catch (reason) { setError(getApiErrorMessage(reason, 'Unable to load delegations.')); } finally { setLoading(false); } }, [query]);
  useEffect(() => { void load(); }, [load]); useEffect(() => { void hrDelegationService.getEntities().then(setEntities).catch(() => setEntities([])); }, []);
  async function open(id: string, mode: 'view' | 'edit') { try { const details = await hrDelegationService.getDetails(id); if (mode === 'view') setPreview(details); else { setEditing(details); setFormOpen(true); } } catch (reason) { toast.error(getApiErrorMessage(reason, 'Unable to load delegation.')); } }
  async function cancel() { if (!cancelTarget || cancelReason.trim().length < 2) return; try { await hrDelegationService.cancel(cancelTarget.id, cancelReason); toast.success('Delegation cancelled.'); setCancelTarget(null); setCancelReason(''); void load(); } catch (reason) { toast.error(getApiErrorMessage(reason, 'Unable to cancel delegation.')); } }
  return <div className="space-y-6">
    <PageHeader actions={<Button fullWidth={false} leftIcon={<Plus className="h-4 w-4" />} onClick={() => { setEditing(null); setFormOpen(true); }}>Create Delegation</Button>} description="Database data → official A4 delegation → preview → print" title="Delegation Generator" />
    <Card className="overflow-hidden p-0">
      <div className="grid gap-3 border-b border-mis-border p-4 lg:grid-cols-[2fr_1fr_1fr_1fr_1fr]"><label className="relative"><Search className="absolute start-3 top-3 h-5 w-5 text-slate-400" /><input className={`${fieldClass} ps-10`} onChange={event => setQuery(current => ({ ...current, search: event.target.value, page: 1 }))} placeholder="Number, employee, National ID, bank/entity" value={query.search} /></label><select className={fieldClass} onChange={event => setQuery(current => ({ ...current, delegatingEntityId: event.target.value, page: 1 }))} value={query.delegatingEntityId}><option value="">All banks / entities</option>{entities.map(entity => <option key={entity.id} value={entity.id}>{entity.nameArabic}</option>)}</select><select className={fieldClass} onChange={event => setQuery(current => ({ ...current, status: event.target.value as DelegationQuery['status'], page: 1 }))} value={query.status}><option value="">All statuses</option>{['Draft', 'Active', 'Expired', 'Cancelled'].map(value => <option key={value}>{value}</option>)}</select><input className={fieldClass} onChange={event => setQuery(current => ({ ...current, dateFrom: event.target.value, page: 1 }))} type="date" value={query.dateFrom} /><input className={fieldClass} min={query.dateFrom || undefined} onChange={event => setQuery(current => ({ ...current, dateTo: event.target.value, page: 1 }))} type="date" value={query.dateTo} /></div>
      {error ? <div className="p-6"><ErrorState compact message={error} onRetry={() => void load()} title="Unable to load delegations" /></div> : loading ? <div className="flex min-h-72 items-center justify-center"><LoadingSpinner /></div> : !data.items.length ? <EmptyState description="Create the first dynamic official delegation." icon={<ShieldX />} title="No delegations found" /> : <><div className="overflow-x-auto"><table className="w-full min-w-[900px]"><thead className="bg-slate-50 text-xs uppercase text-slate-500"><tr>{['Delegation Number', 'Employee', 'Bank / Entity', 'Start Date', 'End Date', 'Status', 'Actions'].map(value => <th className="px-4 py-3 text-start" key={value}>{value}</th>)}</tr></thead><tbody className="divide-y divide-mis-border">{data.items.map(item => <tr key={item.id}><td className="px-4 py-4 font-bold text-mis-navy">{item.delegationNumber}</td><td className="px-4 py-4"><b>{item.employeeName}</b><small className="block text-slate-500">{item.employeeNumber}</small></td><td className="px-4 py-4">{item.authorizedEntity || '—'}</td><td className="px-4 py-4">{displayDate(item.startDate)}</td><td className="px-4 py-4">{displayDate(item.endDate)}</td><td className="px-4 py-4"><StatusBadge tone={statusTone(item.status)}>{item.status}</StatusBadge></td><td className="px-4 py-4"><div className="flex gap-1"><button aria-label="View" className="rounded-lg p-2 text-mis-primary hover:bg-mis-pale" onClick={() => void open(item.id, 'view')}><Eye className="h-4 w-4" /></button><button aria-label="Print" className="rounded-lg p-2 text-mis-primary hover:bg-mis-pale" onClick={() => void open(item.id, 'view')}><Printer className="h-4 w-4" /></button>{item.status !== 'Cancelled' ? <button aria-label="Edit" className="rounded-lg p-2 text-mis-primary hover:bg-mis-pale" onClick={() => void open(item.id, 'edit')}><FilePenLine className="h-4 w-4" /></button> : null}{item.status !== 'Cancelled' ? <button aria-label="Cancel" className="rounded-lg p-2 text-red-600 hover:bg-red-50" onClick={() => setCancelTarget(item)}><ShieldX className="h-4 w-4" /></button> : null}</div></td></tr>)}</tbody></table></div><Pagination onPageChange={page => setQuery(current => ({ ...current, page }))} page={data.page} pageSize={data.pageSize} totalCount={data.totalCount} totalPages={data.totalPages} /></>}
    </Card>
    {formOpen ? <DelegationForm entities={entities} item={editing} onClose={() => { setFormOpen(false); setEditing(null); }} onSaved={saved => { setFormOpen(false); setEditing(null); setPreview(saved); void load(); }} /> : null}
    {preview ? <Preview item={preview} onClose={() => setPreview(null)} onEdit={() => { setEditing(preview); setPreview(null); setFormOpen(true); }} /> : null}
    {cancelTarget ? <Modal footer={<><Button fullWidth={false} onClick={() => setCancelTarget(null)} variant="outline">Close</Button><Button disabled={cancelReason.trim().length < 2} fullWidth={false} onClick={() => void cancel()} variant="danger">Cancel Delegation</Button></>} onClose={() => setCancelTarget(null)} open title="Cancel Delegation"><p className="mb-3 text-sm text-slate-600">This keeps the historical record and audit trail.</p><textarea className="min-h-24 w-full rounded-xl border border-mis-border p-3" onChange={event => setCancelReason(event.target.value)} placeholder="Cancellation reason" value={cancelReason} /></Modal> : null}
  </div>;
}
