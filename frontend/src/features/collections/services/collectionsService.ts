import { apiClient, downloadApiFile, requestFormData } from '../../../services/apiClient';
import type { Activity, AssignmentPreview, AutoAssignmentPreview, BucketConfiguration, CaseDetails, CaseFilters, ClientCard, ClientConfiguration, CollectionAttachment, CollectionCase, CollectionDashboard, CollectorLookup, CollectionAudit, CollectionReport, CollectionsConfiguration, Complaint, FieldVisit, ImportBatch, ImportPreview, PagedResult, PaymentItem, PortfolioConfiguration, PortfolioLookup, PromiseItem, WorkQueue } from '../types/collections';

function params(values: Record<string, unknown>) {
  const query = new URLSearchParams();
  Object.entries(values).forEach(([key, value]) => { if (value !== undefined && value !== null && value !== '') query.set(key, String(value)); });
  return query;
}

export const collectionsService = {
  async dashboard(organizationId?: string) { return (await apiClient.get<CollectionDashboard>('/collections/dashboard', { params: { organizationId } })).data; },
  async clients(values: { page?: number; pageSize?: number; search?: string; type?: string; active?: boolean } = {}) { return (await apiClient.get<PagedResult<ClientCard>>('/collections/clients', { params: values })).data; },
  async cases(values: CaseFilters = {}) { return (await apiClient.get<PagedResult<CollectionCase>>(`/collections/cases?${params(values as Record<string, unknown>)}`)).data; },
  async caseDetails(id: string) { return (await apiClient.get<CaseDetails>(`/collections/cases/${id}`)).data; },
  async revealSensitive(id: string) { return (await apiClient.post<CaseDetails>(`/collections/cases/${id}/reveal-sensitive`)).data; },
  async myWork() { return (await apiClient.get<WorkQueue>('/collections/work-queue/my')).data; },
  async createActivity(id: string, value: { activityType: string; result?: string; notes?: string; channel?: string; nextFollowUpAt?: string }) { return (await apiClient.post<Activity>(`/collections/cases/${id}/activities`, value)).data; },
  async createPromise(id: string, value: { promisedAmount: number; promiseDate: string; channel: string; notes?: string }) { return (await apiClient.post<PromiseItem>(`/collections/cases/${id}/promises`, value)).data; },
  async submitPayment(id: string, value: { amount: number; paymentDate: string; method: string; referenceNumber: string }) { return (await apiClient.post<PaymentItem>(`/collections/cases/${id}/payments`, value)).data; },
  async promises(values: Record<string, unknown> = {}) { return (await apiClient.get<PagedResult<PromiseItem>>(`/collections/promises?${params(values)}`)).data; },
  async payments(values: Record<string, unknown> = {}) { return (await apiClient.get<PagedResult<PaymentItem>>(`/collections/payments?${params(values)}`)).data; },
  async reviewPayment(id: string, approve: boolean, rejectionReason?: string) { return (await apiClient.patch<PaymentItem>(`/collections/payments/${id}/review`, { approve, rejectionReason })).data; },
  async previewAssignment(value: { caseIds: string[]; collectorId: string; teamId?: string; reason: string; confirmed: boolean }) { return (await apiClient.post<AssignmentPreview>('/collections/assignments/preview', value)).data; },
  async assign(value: { caseIds: string[]; collectorId: string; teamId?: string; reason: string; confirmed: boolean }) { return (await apiClient.post<AssignmentPreview>('/collections/assignments', value)).data; },
  async previewAutomaticAssignment(value: { caseIds: string[]; collectorIds?: string[]; teamId?: string; maxActiveCases: number; confirmed: boolean }) { return (await apiClient.post<AutoAssignmentPreview>('/collections/assignments/automatic/preview', value)).data; },
  async applyAutomaticAssignment(value: { caseIds: string[]; collectorIds?: string[]; teamId?: string; maxActiveCases: number; confirmed: boolean }) { return (await apiClient.post<AutoAssignmentPreview>('/collections/assignments/automatic', value)).data; },
  async collectors() { return (await apiClient.get<CollectorLookup[]>('/collections/assignments/collectors')).data; },
  async visits(values: Record<string, unknown> = {}) { return (await apiClient.get<PagedResult<FieldVisit>>(`/collections/visits?${params(values)}`)).data; },
  async createVisit(value: { caseId: string; collectorId: string; scheduledAt: string; address: string; governorate?: string; area?: string }) { return (await apiClient.post<FieldVisit>('/collections/visits', value)).data; },
  async completeVisit(id: string, result: string, notes?: string) { return (await apiClient.patch<FieldVisit>(`/collections/visits/${id}/complete`, { result, notes })).data; },
  async complaints(values: Record<string, unknown> = {}) { return (await apiClient.get<PagedResult<Complaint>>(`/collections/complaints?${params(values)}`)).data; },
  async createComplaint(value: { caseId: string; reference: string; source: string; category: string; severity: string; description: string; receivedAt: string; slaDueAt: string; ownerId: string }) { return (await apiClient.post<Complaint>('/collections/complaints', value)).data; },
  async changeComplaintStatus(id: string, status: string, resolution?: string) { return (await apiClient.patch<Complaint>(`/collections/complaints/${id}/status`, { status, resolution })).data; },
  async audit(values: Record<string, unknown> = {}) { return (await apiClient.get<PagedResult<CollectionAudit>>(`/collections/audit?${params(values)}`)).data; },
  async portfolios(organizationId?: string) { return (await apiClient.get<PortfolioLookup[]>('/collections/imports/portfolios', { params: { organizationId } })).data; },
  async imports(values: Record<string, unknown> = {}) { return (await apiClient.get<PagedResult<ImportBatch>>(`/collections/imports?${params(values)}`)).data; },
  async uploadImport(organizationId: string, portfolioId: string, file: File) { const form = new FormData(); form.append('organizationId', organizationId); form.append('portfolioId', portfolioId); form.append('file', file); return requestFormData<ImportBatch>('/collections/imports', form); },
  async importPreview(id: string, values: Record<string, unknown> = {}) { return (await apiClient.get<ImportPreview>(`/collections/imports/${id}?${params(values)}`)).data; },
  async confirmImport(id: string, notes?: string) { return (await apiClient.post<ImportBatch>(`/collections/imports/${id}/confirm`, { notes })).data; },
  async downloadImportErrors(id: string) { return downloadApiFile(`/collections/imports/${id}/errors.csv`, `collection-import-${id}-errors.csv`); },
  async configuration() { return (await apiClient.get<CollectionsConfiguration>('/collections/configuration')).data; },
  async saveClient(id: string | undefined, value: Omit<ClientConfiguration, 'id'>) { return (await apiClient.request<ClientConfiguration>({ method: id ? 'put' : 'post', url: id ? `/collections/configuration/clients/${id}` : '/collections/configuration/clients', data: value })).data; },
  async uploadClientLogo(id: string, file: File) { const form = new FormData(); form.append('file', file); return requestFormData<{ logoUrl: string }>(`/collections/branding/clients/${id}/logo`, form); },
  async savePortfolio(id: string | undefined, value: Omit<PortfolioConfiguration, 'id'>) { return (await apiClient.request<PortfolioConfiguration>({ method: id ? 'put' : 'post', url: id ? `/collections/configuration/portfolios/${id}` : '/collections/configuration/portfolios', data: value })).data; },
  async saveBucket(id: string | undefined, value: Omit<BucketConfiguration, 'id'>) { return (await apiClient.request<BucketConfiguration>({ method: id ? 'put' : 'post', url: id ? `/collections/configuration/buckets/${id}` : '/collections/configuration/buckets', data: value })).data; },
  async attachments(caseId: string) { return (await apiClient.get<CollectionAttachment[]>(`/collections/attachments/case/${caseId}`)).data; },
  async uploadAttachment(caseId: string, category: string, file: File, paymentId?: string) { const form = new FormData(); form.append('caseId', caseId); form.append('category', category); if (paymentId) form.append('paymentId', paymentId); form.append('file', file); return requestFormData<CollectionAttachment>('/collections/attachments', form); },
  async downloadAttachment(id: string, fileName: string) { return downloadApiFile(`/collections/attachments/${id}/download`, fileName); },
  async executiveReport(values: Record<string, unknown>) { return (await apiClient.get<CollectionReport>(`/collections/reports/executive?${params(values)}`)).data; },
  async exportExecutiveReport(values: Record<string, unknown>) { return downloadApiFile(`/collections/reports/executive.csv?${params(values)}`, 'collections-executive.csv'); },
};
