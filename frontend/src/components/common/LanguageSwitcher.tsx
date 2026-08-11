import { Languages } from 'lucide-react';
import { useLocalization } from '../../context/LocalizationContext';

interface LanguageSwitcherProps {
  className?: string;
}

export function LanguageSwitcher({ className = '' }: LanguageSwitcherProps) {
  const { language, setLanguage, t } = useLocalization();

  return (
    <div aria-label={t('language')} className={`inline-flex items-center gap-1 rounded-xl border border-mis-border bg-white p-1 shadow-sm ${className}`} role="group">
      <Languages className="mx-2 h-4 w-4 text-mis-primary" aria-hidden="true" />
      <button
        aria-pressed={language === 'ar'}
        className={`rounded-lg px-3 py-1.5 text-xs font-semibold transition ${language === 'ar' ? 'bg-mis-pale text-mis-deep' : 'text-slate-500 hover:bg-slate-50'}`}
        lang={language}
        onClick={() => setLanguage('ar')}
        type="button"
      >
        {t('arabic')}
      </button>
      <button
        aria-pressed={language === 'en'}
        className={`rounded-lg px-3 py-1.5 text-xs font-semibold transition ${language === 'en' ? 'bg-mis-pale text-mis-deep' : 'text-slate-500 hover:bg-slate-50'}`}
        lang={language}
        onClick={() => setLanguage('en')}
        type="button"
      >
        {t('english')}
      </button>
    </div>
  );
}
