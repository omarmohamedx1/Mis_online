import type { ReactNode } from 'react';
import misLogo from '../../assets/mis-logo.svg';
import { useLocalization } from '../../context/LocalizationContext';
import { LanguageSwitcher } from '../common/LanguageSwitcher';

interface AuthLayoutProps {
  children: ReactNode;
}

export function AuthLayout({ children }: AuthLayoutProps) {
  const { t } = useLocalization();
  return (
    <main className="min-h-screen overflow-hidden bg-mis-surface text-mis-ink md:grid md:grid-cols-[0.78fr_1fr] lg:grid-cols-[1.12fr_0.88fr]">
      <LanguageSwitcher className="fixed end-5 top-5 z-30" />
      <section className="relative hidden min-h-screen flex-col justify-between overflow-hidden bg-white px-10 py-10 md:flex xl:px-16">
        <div className="absolute inset-0 bg-[linear-gradient(135deg,rgba(231,243,250,0.96),rgba(255,255,255,0.9)_42%,rgba(216,227,236,0.78))]" />
        <div className="absolute inset-y-0 right-0 w-px bg-mis-border" />
        <div className="absolute left-10 top-24 h-56 w-56 rotate-45 border border-mis-sky/30" />
        <div className="absolute bottom-28 left-16 h-44 w-72 border-l-4 border-t-4 border-mis-blue/20" />
        <svg className="absolute bottom-10 right-[-80px] h-[430px] w-[430px] text-mis-deep/10" viewBox="0 0 420 420" fill="none" aria-hidden="true">
          <path d="M58 320C154 322 255 250 360 42" stroke="currentColor" strokeWidth="34" strokeLinecap="round" />
          <path d="M290 60H368V138" stroke="currentColor" strokeWidth="34" strokeLinecap="round" strokeLinejoin="round" />
        </svg>

        <div className="relative z-10">
          <img src={misLogo} alt={t('companyLogoAlt')} className="h-28 w-auto" />
          <div className="mt-10 max-w-md">
            <p className="text-sm font-semibold uppercase text-mis-primary">{t('collectionFirm')}</p>
            <h1 className="mt-4 text-4xl font-bold text-mis-navy">MIS</h1>
            <p className="mt-3 text-xl font-semibold text-mis-deep">{t('collectionSystem')}</p>
            <p className="mt-6 text-base leading-7 text-slate-600">{t('secureTagline')}</p>
          </div>
        </div>

        <div className="relative z-10 flex items-end justify-between gap-6 text-sm text-slate-500">
          <span>{t('internalSystem')}</span>
          <span>&copy; {t('collectionFirm')}</span>
        </div>
      </section>

      <section className="flex min-h-screen items-center justify-center px-5 py-8 sm:px-8">
        <div className="w-full max-w-[460px]">{children}</div>
      </section>
    </main>
  );
}
