import misLogo from '../../../assets/mis-logo.svg';
import type { DelegationDetails } from '../types/delegation';
import './delegation-document.css';

const arabicDateFormatter = new Intl.DateTimeFormat('ar-EG-u-ca-gregory-nu-latn', {
  day: 'numeric',
  month: 'long',
  year: 'numeric',
  timeZone: 'UTC',
});

export function formatDelegationArabicDate(value: string) {
  const match = /^(\d{4})-(\d{2})-(\d{2})$/.exec(value);
  if (!match) return '';
  const date = new Date(Date.UTC(Number(match[1]), Number(match[2]) - 1, Number(match[3])));
  return arabicDateFormatter.format(date).replace(/[،,]/g, '').replace(/\s+/g, ' ').trim();
}

export function DelegationDocument({ delegation }: { delegation: DelegationDetails }) {
  return (
    <article className="delegation-paper" dir="rtl" lang="ar" aria-label={`تفويض ${delegation.delegationNumber}`}>
      <div className="delegation-border">
        <img className="delegation-logo" src={misLogo} alt="MIS Collection Firm" />
        <p className="delegation-date">القاهرة في {formatDelegationArabicDate(delegation.startDate)} حتى {formatDelegationArabicDate(delegation.endDate)}</p>
        <h1>تفويض</h1>
        <section className="delegation-body">
          <p>فوضنا نحن / شركة أم أي أس كولكشن فيرم ويمثلها</p>
          <p>السيد الأستاذ / <strong>{delegation.companyRepresentative || '................................'}</strong> بصفته وكيلاً عن</p>
          <p>بنك / <strong>{delegation.authorizedEntity || '................................'}</strong> بموجب التوكيل رقم: <strong>{delegation.powerOfAttorneyNumber || '........'}</strong> لسنة <strong>{delegation.powerOfAttorneyYear || '........'}</strong></p>
          <p>السيد الأستاذ / <strong>{delegation.employeeName}</strong></p>
          <p>بطاقة رقم قومي / <strong className="latin-value">{delegation.employeeNationalId || '................................'}</strong></p>
          <p className="delegation-authorization">{delegation.purpose}</p>
          <p>وهذا تفويض منا بذلك</p>
        </section>
        <section className="delegation-signature">
          <p>المفوض / <strong>{delegation.employeeName}</strong></p>
          <p>التوقيع /</p>
        </section>
        <footer>العنوان: 7 شارع الجولف – المعادي السرايات - القاهرة</footer>
      </div>
    </article>
  );
}
