import { apiClient } from '../../../services/apiClient';
import type { AuditQuery, PagedAuditLogs } from '../types/audit';

export const hrAuditService = {
  async getPaged(query: AuditQuery): Promise<PagedAuditLogs> {
    const { data } = await apiClient.get<PagedAuditLogs>('/hr/audit', {
      params: {
        action: query.action || undefined,
        employeeId: query.employeeId || undefined,
        entityType: query.entityType || undefined,
        from: query.from || undefined,
        page: query.page,
        pageSize: query.pageSize,
        search: query.search || undefined,
        to: query.to || undefined,
      },
    });
    return data;
  },
};
