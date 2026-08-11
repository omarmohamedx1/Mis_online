export type DocumentExpiryStatus = 'All' | 'Expired' | 'ExpiringSoon' | 'Valid' | 'NoExpiry';

export interface EmployeeDocumentListItem {
  id: string;
  employeeId: string;
  employeeNumber: string;
  employeeName: string;
  departmentName: string;
  documentTypeId: string | null;
  documentType: string;
  fileName: string;
  mimeType: string;
  fileSize: number;
  issueDate: string | null;
  expiryDate: string | null;
  expiryStatus: Exclude<DocumentExpiryStatus, 'All'>;
  daysUntilExpiry: number | null;
  uploadedBy: string;
  uploadedAt: string;
  updatedAt: string | null;
}

export interface EmployeeDocumentDetails extends Omit<EmployeeDocumentListItem, 'departmentName'> {
  sha256Hash: string | null;
  notes: string | null;
  uploadedByUserId: string;
}

export interface PagedEmployeeDocuments {
  items: EmployeeDocumentListItem[];
  totalCount: number;
  page: number;
  pageSize: number;
  totalPages: number;
}

export interface DocumentExpirySummary {
  expired: number;
  expiringWithin7Days: number;
  expiringWithin15Days: number;
  expiringWithin30Days: number;
}

export interface EmployeeDocumentQuery {
  page: number;
  pageSize: number;
  search: string;
  employeeId: string;
  departmentId: string;
  documentTypeId: string;
  expiryStatus: DocumentExpiryStatus;
  expiringWithinDays: number;
  sortBy: string;
  sortDirection: 'asc' | 'desc';
}

export interface SaveEmployeeDocumentMetadata {
  documentTypeId: string;
  issueDate: string | null;
  expiryDate: string | null;
  notes: string | null;
}
