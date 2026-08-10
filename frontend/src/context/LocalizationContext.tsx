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

    const lookup = new Map<string, string>();
    (Object.keys(translations.en) as TranslationKey[]).forEach((key) => {
      lookup.set(translations.en[key], translations[language][key]);
      lookup.set(translations.ar[key], translations[language][key]);
    });
    const fragments: Record<string, string> = language === 'ar'
      ? { 'Showing': 'عرض', 'Page': 'صفحة', 'of': 'من', 'Select employee': 'اختر الموظف', 'Select department': 'اختر القسم', 'All departments': 'كل الأقسام', 'All statuses': 'كل الحالات', 'All': 'الكل', 'Action': 'الإجراء', 'Actions': 'الإجراءات' }
      : { 'عرض': 'Showing', 'صفحة': 'Page', 'من': 'of', 'اختر الموظف': 'Select employee', 'اختر القسم': 'Select department', 'كل الأقسام': 'All departments', 'كل الحالات': 'All statuses', 'الكل': 'All', 'الإجراء': 'Action', 'الإجراءات': 'Actions' };
    Object.entries(fragments).forEach(([source, target]) => lookup.set(source, target));

    const translateElement = (root: ParentNode) => {
      const walker = document.createTreeWalker(root, NodeFilter.SHOW_TEXT);
      let node = walker.nextNode();
      while (node) {
        const original = node.textContent ?? '';
        const trimmed = original.trim();
        const translated = lookup.get(trimmed);
        if (translated && translated !== trimmed) node.textContent = original.replace(trimmed, translated);
        node = walker.nextNode();
      }
      root.querySelectorAll?.('input[placeholder], textarea[placeholder], [title]').forEach((element) => {
        for (const attribute of ['placeholder', 'title']) {
          const current = element.getAttribute(attribute);
          const translated = current ? lookup.get(current) : null;
          if (translated) element.setAttribute(attribute, translated);
        }
      });
    };
    translateElement(document.body);
    const observer = new MutationObserver((mutations) => mutations.forEach((mutation) => mutation.addedNodes.forEach((node) => {
      if (node.nodeType === Node.ELEMENT_NODE) translateElement(node as Element);
      if (node.nodeType === Node.TEXT_NODE && node.parentElement) translateElement(node.parentElement);
    })));
    observer.observe(document.body, { childList: true, subtree: true });
    return () => observer.disconnect();
  }, [language, isRtl]);
  const value = useMemo<LocalizationValue>(() => ({ language, isRtl, setLanguage, t: (key, values) => { let text: string = translations[language][key]; Object.entries(values ?? {}).forEach(([name, replacement]) => { text = text.replaceAll(`{${name}}`, String(replacement)); }); return text; } }), [language, isRtl]);
  return <LocalizationContext.Provider value={value}>{children}</LocalizationContext.Provider>;
}

export function useLocalization() { const context = useContext(LocalizationContext); if (!context) throw new Error('useLocalization must be used within LocalizationProvider.'); return context; }
