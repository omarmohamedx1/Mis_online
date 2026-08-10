import { Clock3 } from 'lucide-react';
import { useLocalization } from '../../context/LocalizationContext';
import type { TranslationKey } from '../../localization/translations';

interface HrPageProps { titleKey: TranslationKey; }

export function HrPage({ titleKey }: HrPageProps) {
  const { t } = useLocalization();
  return (
    <section className="mx-auto max-w-7xl">
      <div className="mb-7"><p className="text-sm font-semibold text-mis-primary">{t('hrDepartment')}</p><h1 className="mt-2 text-3xl font-bold text-mis-navy">{t(titleKey)}</h1></div>
      <div className="flex min-h-[360px] items-center justify-center rounded-2xl border border-mis-border bg-white p-8 text-center shadow-sm">
        <div className="max-w-md"><div className="mx-auto flex h-14 w-14 items-center justify-center rounded-2xl bg-mis-pale text-mis-primary"><Clock3 className="h-7 w-7" /></div><h2 className="mt-5 text-lg font-bold text-mis-navy">{t('moduleComingSoon')}</h2><p className="mt-2 text-sm leading-6 text-slate-500">{t('moduleComingHelp')}</p></div>
      </div>
    </section>
  );
}
