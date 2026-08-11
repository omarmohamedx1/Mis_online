export type HrReportExportFormat = 'excel' | 'pdf';

export interface HrReportCatalogItem {
  code: string;
  name: string;
  description: string;
  supportedFilters: string[];
}

export interface HrReportColumn {
  key: string;
  header: string;
}

export interface HrReportRow {
  values: Record<string, string | null>;
}

export interface HrReportPreview {
  reportCode: string;
  reportName: string;
  columns: HrReportColumn[];
  rows: HrReportRow[];
  appliedFilters: Record<string, string>;
  totalCount: number;
  page: number;
  pageSize: number;
  totalPages: number;
  generatedAt: string;
}

export interface HrReportFilter {
  page: number;
  pageSize: number;
  search: string;
  dateFrom: string;
  dateTo: string;
  employeeId: string;
  departmentId: string;
  branchId: string;
  status: string;
  typeId: string;
  type: string;
}

export const emptyHrReportFilter = (): HrReportFilter => ({
  page: 1,
  pageSize: 50,
  search: '',
  dateFrom: '',
  dateTo: '',
  employeeId: '',
  departmentId: '',
  branchId: '',
  status: '',
  typeId: '',
  type: '',
});
