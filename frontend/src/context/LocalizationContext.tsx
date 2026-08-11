import { createContext, useContext, useEffect, useMemo, useState, type ReactNode } from 'react';
import { translations, type Language, type TranslationKey } from '../localization/translations';

const STORAGE_KEY = 'mis.language';
interface LocalizationValue { language: Language; isRtl: boolean; setLanguage: (language: Language) => void; t: (key: TranslationKey, values?: Record<string, string | number>) => string; }
const LocalizationContext = createContext<LocalizationValue | null>(null);

export function LocalizationProvider({ children }: { children: ReactNode }) {
  const [language, setLanguage] = useState<Language>(() => localStorage.getItem(STORAGE_KEY) === 'ar' ? 'ar' : 'en');
  const isRtl = language === 'ar';
  useEffect(() => {
    localStorage.setItem(STORAGE_KEY, language);
    document.documentElement.lang = language;
    document.documentElement.dir = isRtl ? 'rtl' : 'ltr';
    document.body.dir = isRtl ? 'rtl' : 'ltr';
    document.title = translations[language].collectionFirm;
  }, [language, isRtl]);
  const value = useMemo<LocalizationValue>(() => ({ language, isRtl, setLanguage, t: (key, values) => { let text: string = translations[language][key]; Object.entries(values ?? {}).forEach(([name, replacement]) => { text = text.replaceAll(`{${name}}`, String(replacement)); }); return text; } }), [language, isRtl]);
  return <LocalizationContext.Provider value={value}>{children}</LocalizationContext.Provider>;
}

export function useLocalization() { const context = useContext(LocalizationContext); if (!context) throw new Error('useLocalization must be used within LocalizationProvider.'); return context; }
