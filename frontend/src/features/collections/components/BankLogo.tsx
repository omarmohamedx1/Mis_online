import { Building2 } from 'lucide-react';
import { env } from '../../../config/env';

interface BankLogoProps { code: string; name: string; logoUrl?: string; className?: string }
const palettes = ['from-sky-600 to-blue-900', 'from-emerald-500 to-teal-800', 'from-violet-500 to-indigo-900', 'from-amber-500 to-orange-800', 'from-rose-500 to-red-900'];
function absoluteLogoUrl(value: string) { if (/^https?:\/\//i.test(value)) return value; return `${env.apiUrl.replace(/\/api\/?$/, '')}${value.startsWith('/') ? value : `/${value}`}`; }

export function BankLogo({ code, name, logoUrl, className = 'h-16 w-16' }: BankLogoProps) {
  const palette = palettes[Array.from(code).reduce((sum, char) => sum + char.charCodeAt(0), 0) % palettes.length];
  if (logoUrl) return <span className={`grid shrink-0 place-items-center overflow-hidden rounded-2xl border border-slate-200 bg-white p-2 shadow-sm ${className}`}><img src={absoluteLogoUrl(logoUrl)} alt={`${name} logo`} className="h-full w-full object-contain" loading="lazy" /></span>;
  const initials = code.replace(/[^A-Z0-9]/gi, '').slice(0, 3).toUpperCase();
  return <span title={name} className={`relative grid shrink-0 place-items-center overflow-hidden rounded-2xl bg-gradient-to-br text-white shadow-sm ${palette} ${className}`}><Building2 className="absolute h-10 w-10 opacity-15" /><strong className="relative text-sm tracking-wider">{initials || 'BANK'}</strong></span>;
}
