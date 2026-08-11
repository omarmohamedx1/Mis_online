export interface AuditChange {
  field: string;
  oldValue: string | null;
  newValue: string | null;
}

export interface AuditLogItem {
  id: string;
  userId: string;
  username: string;
  action: string;
  entityType: string;
  entityId: string;
  employeeId: string | null;
  employeeName: string | null;
  description: string | null;
  changes: AuditChange[];
  timestamp: string;
}

export interface PagedAuditLogs {
  items: AuditLogItem[];
  totalCount: number;
  page: number;
  pageSize: number;
  totalPages: number;
}

export interface AuditQuery {
  page: number;
  pageSize: number;
  search?: string;
  action?: string;
  entityType?: string;
  employeeId?: string;
  from?: string;
  to?: string;
}
