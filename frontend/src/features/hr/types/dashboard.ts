export interface DepartmentEmployeeCount {
  departmentId: string;
  departmentName: string;
  departmentCode: string;
  employeeCount: number;
}

export interface HrDashboardSummary {
  totalEmployees: number;
  activeEmployees: number;
  absentToday: number | null;
  attendanceAvailable: boolean;
  documentsRequiringAttention: number | null;
  documentAttentionAvailable: boolean;
  totalDocuments: number;
  employeesByDepartment: DepartmentEmployeeCount[];
}
