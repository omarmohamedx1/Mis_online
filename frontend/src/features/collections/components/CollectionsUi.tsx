import type { ReactNode } from 'react';
import { useCollectionsLocalization } from '../localization/collectionsTranslations';

export function KpiCard({ label, value, accent = 'blue', hint }: { label: ReactNode; value: ReactNode; accent?: 'blue' | 'green' | 'amber' | 'red'; hint?: ReactNode }) {
  const colors = { blue: 'border-mis-sky/60 bg-white', green: 'border-emerald-200 bg-emerald-50/30', amber: 'border-amber-200 bg-amber-50/30', red: 'border-rose-200 bg-rose-50/30' };
  return <article className={`rounded-2xl border p-5 shadow-sm ${colors[accent]}`}><p className="text-sm font-medium text-slate-500">{label}</p><p className="mt-2 text-2xl font-bold tabular-nums text-mis-navy" data-bidi="ltr">{value}</p>{hint ? <p className="mt-1 text-xs text-slate-500">{hint}</p> : null}</article>;
}

export function CollectionStatus({ value }: { value: string }) {
  const { ct } = useCollectionsLocalization(); const tone = ['APPROVED', 'FULFILLED', 'HEALTHY'].includes(value) ? 'bg-emerald-100 text-emerald-800' : ['BROKEN', 'REJECTED', 'AT_RISK', 'HIGH'].includes(value) ? 'bg-rose-100 text-rose-800' : ['DUE_TODAY', 'PARTIALLY_FULFILLED', 'WATCH', 'MEDIUM'].includes(value) ? 'bg-amber-100 text-amber-800' : 'bg-sky-100 text-sky-800';
  return <span className={`inline-flex rounded-full px-2.5 py-1 text-xs font-bold ${tone}`}>{ct(value)}</span>;
}

export function useCollectionFormat() {
  const { language, ct } = useCollectionsLocalization(); const locale = language === 'ar' ? 'ar-EG' : 'en-EG';
  return {
    money: (value: number) => new Intl.NumberFormat(locale, { maximumFractionDigits: 0 }).format(value) + ` ${ct('currency')}`,
    number: (value: number) => new Intl.NumberFormat(locale).format(value),
    date: (value?: string) => value ? new Intl.DateTimeFormat(locale, { dateStyle: 'medium' }).format(new Date(value)) : '—',
    dateTime: (value?: string) => value ? new Intl.DateTimeFormat(locale, { dateStyle: 'medium', timeStyle: 'short' }).format(new Date(value)) : '—',
  };
}
