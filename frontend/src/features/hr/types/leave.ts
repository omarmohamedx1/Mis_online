export const leaveRequestStatuses = ['Pending', 'Approved', 'Rejected', 'Cancelled'] as const;
export type LeaveRequestStatus = (typeof leaveRequestStatuses)[number];

export interface LeaveRequestListItem {
  id: string;
  employeeId: string;
  employeeNumber: string;
  employeeName: string;
  departmentId: string;
  departmentName: string;
  branchId: string | null;
  branchName: string | null;
  leaveTypeId: string;
  leaveTypeName: string;
  startDate: string;
  endDate: string;
  numberOfDays: number;
  requestDate: string;
  status: LeaveRequestStatus;
}

export interface LeaveRequestDetails extends LeaveRequestListItem {
  reason: string | null;
  notes: string | null;
  attachmentDocumentId: string | null;
  attachmentFileName: string | null;
  createdByUserId: string;
  createdByUsername: string;
  decidedByUserId: string | null;
  decidedByUsername: string | null;
  decidedAt: string | null;
  decisionNotes: string | null;
  createdAt: string;
  updatedAt: string | null;
}

export interface PagedLeaveRequests {
  items: LeaveRequestListItem[];
  totalCount: number;
  page: number;
  pageSize: number;
  totalPages: number;
}

export interface LeaveRequestQuery {
  page: number;
  pageSize: number;
  search?: string;
  employeeId?: string;
  departmentId?: string;
  branchId?: string;
  leaveTypeId?: string;
  status?: LeaveRequestStatus | '';
  dateFrom?: string;
  dateTo?: string;
  sortBy?: string;
  sortDescending?: boolean;
}

export interface SaveLeaveRequest {
  employeeId: string;
  leaveTypeId: string;
  startDate: string;
  endDate: string;
  reason: string | null;
  notes: string | null;
  attachmentDocumentId: string | null;
}

export interface LeaveBalance {
  employeeId: string;
  employeeNumber: string;
  employeeName: string;
  leaveTypeId: string;
  leaveTypeName: string;
  year: number;
  entitled: number;
  used: number;
  pending: number;
  remaining: number;
  asOfDate: string;
}

export interface PagedLeaveBalances {
  items: LeaveBalance[];
  totalCount: number;
  page: number;
  pageSize: number;
  totalPages: number;
}

export interface LeaveBalanceQuery {
  page: number;
  pageSize: number;
  search?: string;
  employeeId?: string;
  departmentId?: string;
  branchId?: string;
  leaveTypeId?: string;
  year: number;
}

export interface UpsertLeaveEntitlementRequest {
  baseEntitlement: number;
  adjustment: number;
  notes: string | null;
}

export interface LeaveEntitlement {
  id: string;
  employeeId: string;
  employeeNumber: string;
  employeeName: string;
  leaveTypeId: string;
  leaveTypeName: string;
  year: number;
  baseEntitlement: number;
  adjustment: number;
  totalEntitlement: number;
  notes: string | null;
  createdByUserId: string;
  createdByUsername: string;
  updatedByUserId: string | null;
  updatedByUsername: string | null;
  createdAt: string;
  updatedAt: string | null;
}

