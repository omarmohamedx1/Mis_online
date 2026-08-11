export type DelegationStatus = 'Draft' | 'Active' | 'Expired' | 'Cancelled';

export interface DelegationListItem {
  id: string; delegationNumber: string; employeeId: string; employeeNumber: string; employeeName: string; departmentName: string;
  delegationTypeId: string; delegationType: string; subject: string; authorizedEntity: string | null; startDate: string; endDate: string;
  status: DelegationStatus; createdBy: string; createdAt: string;
}

export interface DelegationDetails extends DelegationListItem {
  employeeNationalId: string | null; purpose: string; notes: string | null; createdByUserId: string; updatedAt: string | null;
  cancellationReason: string | null; cancelledAt: string | null;
}

export interface PagedDelegations { items: DelegationListItem[]; totalCount: number; page: number; pageSize: number; totalPages: number; }
export interface DelegationQuery { page: number; pageSize: number; search: string; employeeId: string; departmentId: string; delegationTypeId: string; status: '' | DelegationStatus; dateFrom: string; dateTo: string; sortBy: string; sortDirection: 'asc' | 'desc'; }
export interface SaveDelegationRequest { delegationNumber?: string | null; employeeId: string; delegationTypeId: string; subject: string; authorizedEntity: string | null; startDate: string; endDate: string; purpose: string; notes: string | null; status: Exclude<DelegationStatus, 'Cancelled'>; }
