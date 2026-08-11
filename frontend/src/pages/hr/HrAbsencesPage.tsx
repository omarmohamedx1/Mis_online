import { CalendarX2, Eye, Pencil, Plus, Search, Trash2 } from 'lucide-react';
import { useCallback, useEffect, useMemo, useState, type ReactNode } from 'react';
import { useSearchParams } from 'react-router-dom';
import { Button } from '../../components/common/Button';
import { EmptyState } from '../../components/common/EmptyState';
import { ErrorState } from '../../components/common/ErrorState';
import { FormError } from '../../components/common/FormError';
import { LoadingSpinner } from '../../components/common/LoadingSpinner';
import { Modal } from '../../components/common/Modal';
import { PageHeader } from '../../components/common/PageHeader';
import { Pagination } from '../../components/common/Pagination';
import { StatusBadge, type StatusTone } from '../../components/common/StatusBadge';
import { useToast } from '../../components/common/Toast';
import { DateInput } from '../../components/forms/DateInput';
import { SelectInput } from '../../components/forms/SelectInput';
import { TextAreaInput } from '../../components/forms/TextAreaInput';
import { useLocalization } from '../../context/LocalizationContext';
import { EmployeeSearchSelect } from '../../features/hr/components/EmployeeSearchSelect';
import { hrAbsenceService } from '../../features/hr/services/hrAbsenceService';
import { hrEmployeeService } from '../../features/hr/services/hrEmployeeService';
import type { AbsenceDetails, AbsenceListItem, AbsenceStatus, PagedAbsences, SaveAbsenceRequest } from '../../features/hr/types/absence';
import type { DepartmentOption } from '../../features/hr/types/employee';
import type { TranslationKey } from '../../localization/translations';
import { getApiErrorMessage } from '../../services/apiClient';

const emptyPage: PagedAbsences = { items: [], totalCount: 0, page: 1, pageSize: 20, totalPages: 0 };
const emptyForm: SaveAbsenceRequest = { employeeId: '', absenceDate: '', type: 'Absent', reason: '', status: 'Pending', notes: '', attendanceSource: 'Manual' };
const statuses: AbsenceStatus[] = ['Pending', 'Excused', 'Unexcused'];
const statusLabels: Record<AbsenceStatus, TranslationKey> = { Pending: 'pending', Excused: 'excused', Unexcused: 'unexcused' };
const statusTones: Record<AbsenceStatus, StatusTone> = { Pending: 'warning', Excused: 'success', Unexcused: 'danger' };

function displayDate(value: string, language: 'en' | 'ar'): string {
  return new Intl.DateTimeFormat(language === 'ar' ? 'ar-EG' : 'en-GB', { dateStyle: 'medium' }).format(new Date(`${value}T12:00:00`));
}

function AbsenceForm({ absence, onClose, onSaved }: { absence: AbsenceDetails | null; onClose: () => void; onSaved: () => void }) {
  const { t } = useLocalization();
  const toast = useToast();
  const formId = 'absence-form';
  const [form, setForm] = useState<SaveAbsenceRequest>(() => absence ? {
    employeeId: absence.employeeId,
    absenceDate: absence.absenceDate,
    type: 'Absent',
    reason: absence.reason ?? '',
    status: absence.status,
    notes: absence.notes ?? '',
    attendanceSource: 'Manual',
  } : { ...emptyForm });
  const [error, setError] = useState('');
  const [saving, setSaving] = useState(false);

  async function submit(event: React.FormEvent<HTMLFormElement>) {
    event.preventDefault();
    setError('');
    if (!form.employeeId || !form.absenceDate) {
      setError(t('employeeDateRequired'));
      return;
    }

    setSaving(true);
    try {
      if (absence) {
        await hrAbsenceService.updateAbsence(absence.id, form);
        toast.success(t('absenceUpdatedSuccess'));
      } else {
        await hrAbsenceService.createAbsence(form);
        toast.success(t('absenceCreatedSuccess'));
      }
      onSaved();
    } catch (requestError) {
      setError(getApiErrorMessage(requestError, t('saveAbsenceError')));
    } finally {
      setSaving(false);
    }
  }

  const initialEmployee = absence ? { id: absence.employeeId, employeeNumber: absence.employeeNumber, fullName: absence.employeeName } : null;

  return (
    <Modal
      closeOnBackdrop={!saving}
      closeOnEscape={!saving}
      footer={(
        <>
          <Button disabled={saving} fullWidth={false} onClick={onClose} type="button" variant="outline">{t('cancel')}</Button>
          <Button form={formId} fullWidth={false} isLoading={saving} type="submit">{absence ? t('saveChanges') : t('recordAbsence')}</Button>
        </>
      )}
      hideCloseButton={saving}
      onClose={onClose}
      open
      title={t(absence ? 'editAbsence' : 'recordAbsence')}
    >
      <form className="space-y-5" id={formId} onSubmit={submit}>
        <FormError message={error} />
        <EmployeeSearchSelect
          includeInactive
          initialSelection={initialEmployee}
          label={t('employee')}
          onChange={(employeeId) => setForm((current) => ({ ...current, employeeId }))}
          required
          value={form.employeeId}
        />
        <DateInput label={t('date')} onChange={(event) => setForm((current) => ({ ...current, absenceDate: event.target.value }))} required value={form.absenceDate} />
        <SelectInput label={t('type')} value="Absent" disabled><option value="Absent">{t('absent')}</option></SelectInput>
        <TextAreaInput label={t('reason')} maxLength={500} onChange={(event) => setForm((current) => ({ ...current, reason: event.target.value }))} rows={3} value={form.reason} />
        <SelectInput label={t('status')} onChange={(event) => setForm((current) => ({ ...current, status: event.target.value as AbsenceStatus }))} value={form.status}>
          {statuses.map((status) => <option key={status} value={status}>{t(statusLabels[status])}</option>)}
        </SelectInput>
        <TextAreaInput label={t('notes')} maxLength={2000} onChange={(event) => setForm((current) => ({ ...current, notes: event.target.value }))} rows={4} value={form.notes} />
        <SelectInput label={t('attendanceSource')} value="Manual" disabled><option value="Manual">{t('manual')}</option></SelectInput>
      </form>
    </Modal>
  );
}

function DetailField({ label, value, wide = false }: { label: string; value: ReactNode; wide?: boolean }) {
  return <div className={`rounded-xl bg-mis-surface p-4 ${wide ? 'sm:col-span-2' : ''}`}><dt className="text-xs font-semibold text-slate-500">{label}</dt><dd className="mt-1.5 break-words text-sm font-semibold text-mis-navy" dir="auto">{value}</dd></div>;
}

function DetailsModal({ absence, onClose }: { absence: AbsenceDetails; onClose: () => void }) {
  const { language, t } = useLocalization();
  return (
    <Modal footer={<Button fullWidth={false} onClick={onClose}>{t('close')}</Button>} onClose={onClose} open title={t('absenceDetails')}>
      <dl className="grid gap-4 sm:grid-cols-2">
        <DetailField label={t('employee')} value={absence.employeeName} />
        <DetailField label={t('employeeId')} value={absence.employeeNumber} />
        <DetailField label={t('department')} value={absence.departmentName} />
        <DetailField label={t('date')} value={displayDate(absence.absenceDate, language)} />
        <DetailField label={t('type')} value={t('absent')} />
        <DetailField label={t('status')} value={<StatusBadge tone={statusTones[absence.status]}>{t(statusLabels[absence.status])}</StatusBadge>} />
        <DetailField label={t('reason')} value={absence.reason || t('unknown')} />
        <DetailField label={t('attendanceSource')} value={t('manual')} />
        <DetailField label={t('notes')} value={absence.notes || '—'} wide />
      </dl>
    </Modal>
  );
}

export function HrAbsencesPage() {
  const { language, t } = useLocalization();
  const toast = useToast();
  const [searchParams] = useSearchParams();
  const [data, setData] = useState(emptyPage);
  const [departments, setDepartments] = useState<DepartmentOption[]>([]);
  const [searchInput, setSearchInput] = useState(() => searchParams.get('employee') ?? '');
  const [search, setSearch] = useState(() => searchParams.get('employee') ?? '');
  const [departmentId, setDepartmentId] = useState('');
  const [date, setDate] = useState('');
  const [status, setStatus] = useState('all');
  const [page, setPage] = useState(1);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');
  const [formRecord, setFormRecord] = useState<AbsenceDetails | null | undefined>(undefined);
  const [details, setDetails] = useState<AbsenceDetails | null>(null);
  const [deleteTarget, setDeleteTarget] = useState<AbsenceListItem | null>(null);
  const [deleting, setDeleting] = useState(false);

  useEffect(() => {
    const timer = window.setTimeout(() => {
      setSearch(searchInput.trim());
      setPage(1);
    }, 350);
    return () => window.clearTimeout(timer);
  }, [searchInput]);

  useEffect(() => {
    void hrEmployeeService.getDepartments().then(setDepartments).catch(() => setError(t('loadDepartmentsError')));
  }, [language, t]);

  const load = useCallback(async () => {
    setLoading(true);
    setError('');
    try {
      setData(await hrAbsenceService.getAbsences({ page, pageSize: 20, search, departmentId, date, status }));
    } catch (requestError) {
      setError(getApiErrorMessage(requestError, t('loadAbsencesError')));
    } finally {
      setLoading(false);
    }
  }, [date, departmentId, language, page, search, status, t]);

  useEffect(() => { void load(); }, [load]);

  async function openRecord(item: AbsenceListItem, mode: 'view' | 'edit') {
    try {
      const record = await hrAbsenceService.getAbsence(item.id);
      if (mode === 'view') setDetails(record);
      else setFormRecord(record);
    } catch (requestError) {
      setError(getApiErrorMessage(requestError, t('loadAbsenceError')));
    }
  }

  async function confirmDelete() {
    if (!deleteTarget) return;
    setDeleting(true);
    try {
      await hrAbsenceService.deleteAbsence(deleteTarget.id);
      toast.success(t('absenceDeletedSuccess'));
      setDeleteTarget(null);
      await load();
    } catch (requestError) {
      setError(getApiErrorMessage(requestError, t('deleteAbsenceError')));
    } finally {
      setDeleting(false);
    }
  }

  const hasFilters = Boolean(search || departmentId || date || status !== 'all');
  const headings = useMemo(() => [t('employeeId'), t('employee'), t('department'), t('date'), t('status'), t('actions')], [t]);

  return (
    <div className="mx-auto max-w-7xl">
      <PageHeader
        actions={<Button fullWidth={false} leftIcon={<Plus className="h-4 w-4" />} onClick={() => setFormRecord(null)}>{t('recordAbsence')}</Button>}
        description={t('absencesSubtitle')}
        eyebrow={t('hrDepartment')}
        title={t('absencesTitle')}
      />
      <section className="overflow-hidden rounded-2xl border border-mis-border bg-white shadow-sm">
        <div className="grid gap-3 border-b border-mis-border p-4 md:grid-cols-2 xl:grid-cols-[minmax(220px,1fr)_180px_170px_150px]">
          <label className="relative">
            <Search className="absolute start-3 top-3 h-5 w-5 text-slate-400" aria-hidden="true" />
            <input className="h-11 w-full rounded-xl border border-mis-border pe-3 ps-10 text-sm outline-none focus:border-mis-blue" onChange={(event) => setSearchInput(event.target.value)} placeholder={t('searchEmployee')} value={searchInput} />
          </label>
          <select aria-label={t('department')} className="h-11 rounded-xl border border-mis-border bg-white px-3 text-sm" onChange={(event) => { setDepartmentId(event.target.value); setPage(1); }} value={departmentId}>
            <option value="">{t('allDepartments')}</option>
            {departments.map((department) => <option key={department.id} value={department.id}>{department.name}</option>)}
          </select>
          <input aria-label={t('absenceDateFilter')} className="h-11 rounded-xl border border-mis-border px-3 text-sm" onChange={(event) => { setDate(event.target.value); setPage(1); }} type="date" value={date} />
          <select aria-label={t('status')} className="h-11 rounded-xl border border-mis-border bg-white px-3 text-sm" onChange={(event) => { setStatus(event.target.value); setPage(1); }} value={status}>
            <option value="all">{t('all')}</option>
            {statuses.map((item) => <option key={item} value={item.toLowerCase()}>{t(statusLabels[item])}</option>)}
          </select>
        </div>

        {error ? <div className="p-5"><ErrorState compact message={error} onRetry={() => void load()} title={t('loadAbsencesError')} /></div> : loading ? (
          <div className="flex min-h-72 items-center justify-center"><LoadingSpinner /></div>
        ) : data.items.length === 0 ? (
          <EmptyState
            action={!hasFilters ? <Button fullWidth={false} leftIcon={<Plus className="h-4 w-4" />} onClick={() => setFormRecord(null)}>{t('recordAbsence')}</Button> : undefined}
            description={hasFilters ? t('adjustFilters') : t('addFirstAbsence')}
            icon={<CalendarX2 />}
            title={t('noAbsencesFound')}
          />
        ) : (
          <>
            <div className="overflow-x-auto">
              <table className="w-full min-w-[850px] text-start">
                <thead className="bg-mis-surface text-xs uppercase tracking-wide text-slate-500"><tr>{headings.map((heading) => <th className="px-5 py-4 text-start" key={heading}>{heading}</th>)}</tr></thead>
                <tbody className="divide-y divide-mis-border">
                  {data.items.map((item) => (
                    <tr className="hover:bg-slate-50/70" key={item.id}>
                      <td className="px-5 py-4 text-sm font-semibold text-mis-navy">{item.employeeNumber}</td>
                      <td className="px-5 py-4 text-sm text-slate-700">{item.employeeName}</td>
                      <td className="px-5 py-4 text-sm text-slate-600">{item.departmentName}</td>
                      <td className="px-5 py-4 text-sm text-slate-600">{displayDate(item.absenceDate, language)}</td>
                      <td className="px-5 py-4"><StatusBadge tone={statusTones[item.status]}>{t(statusLabels[item.status])}</StatusBadge></td>
                      <td className="px-5 py-4"><div className="flex justify-end gap-1">
                        <button aria-label={t('view')} className="rounded-lg p-2 text-mis-primary hover:bg-mis-pale" onClick={() => void openRecord(item, 'view')} type="button"><Eye className="h-4 w-4" /></button>
                        <button aria-label={t('edit')} className="rounded-lg p-2 text-mis-primary hover:bg-mis-pale" onClick={() => void openRecord(item, 'edit')} type="button"><Pencil className="h-4 w-4" /></button>
                        <button aria-label={t('delete')} className="rounded-lg p-2 text-red-600 hover:bg-red-50" onClick={() => setDeleteTarget(item)} type="button"><Trash2 className="h-4 w-4" /></button>
                      </div></td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>
            <Pagination onPageChange={setPage} page={data.page} pageSize={data.pageSize} totalCount={data.totalCount} totalPages={data.totalPages} />
          </>
        )}
      </section>

      {formRecord !== undefined ? <AbsenceForm absence={formRecord} onClose={() => setFormRecord(undefined)} onSaved={() => { setFormRecord(undefined); void load(); }} /> : null}
      {details ? <DetailsModal absence={details} onClose={() => setDetails(null)} /> : null}
      {deleteTarget ? (
        <Modal
          closeOnBackdrop={!deleting}
          closeOnEscape={!deleting}
          footer={(
            <>
              <Button disabled={deleting} fullWidth={false} onClick={() => setDeleteTarget(null)} variant="outline">{t('cancel')}</Button>
              <Button fullWidth={false} isLoading={deleting} onClick={() => void confirmDelete()} variant="danger">{t('delete')}</Button>
            </>
          )}
          hideCloseButton={deleting}
          onClose={() => setDeleteTarget(null)}
          open
          title={t('deleteAbsence')}
        >
          <p className="text-sm leading-6 text-slate-600">{t('deleteConfirmation', { name: deleteTarget.employeeName, date: displayDate(deleteTarget.absenceDate, language) })}</p>
        </Modal>
      ) : null}
    </div>
  );
}
