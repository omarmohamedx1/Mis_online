import { CheckCircle2 } from 'lucide-react';
import { useLocation, useOutletContext } from 'react-router-dom';
import { EmptyState } from '../../components/common/EmptyState';
import { useCollectionsLocalization } from '../../features/collections/localization/collectionsTranslations';
import type { CollectionsTranslationKey } from '../../features/collections/localization/collectionsTranslations';
import type { BankWorkspaceContext } from './BankWorkspaceLayout';

const labels: Record<string, CollectionsTranslationKey> = {
  overview: 'overview', import: 'importData', portfolio: 'portfolio', distribution: 'distribution', ptp: 'ptp', dcr: 'dcr',
  visits: 'visits', complaints: 'complaints', activity: 'activity', archive: 'archive',
};

export function BankWorkspaceSectionPage() {
  const section = useLocation().pathname.split('/').filter(Boolean).at(-1) ?? 'overview';
  const { bank } = useOutletContext<BankWorkspaceContext>();
  const { language, ct } = useCollectionsLocalization();
  const bankName = language === 'ar' ? bank.nameArabic : bank.nameEnglish;
  const label = labels[section] ?? 'overview';

  return (
    <section className="rounded-[2rem] border border-mis-border bg-white shadow-sm">
      <EmptyState
        className="min-h-[340px]"
        icon={<CheckCircle2 />}
        title={ct(label)}
        description={<><span>{ct('workspaceReady')}</span><span className="mt-1 block text-xs font-semibold text-slate-400">{bankName}</span></>}
      />
    </section>
  );
}
