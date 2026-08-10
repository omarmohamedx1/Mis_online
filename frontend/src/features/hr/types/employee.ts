export interface DepartmentOption { id: string; name: string; code: string; }
export interface EmployeeListItem { id: string; employeeNumber: string; fullName: string; departmentId: string; departmentName: string; departmentCode: string; isActive: boolean; }
export interface EmployeeDetails extends EmployeeListItem { createdAt: string; updatedAt: string | null; }
export interface PagedEmployees { items: EmployeeListItem[]; totalCount: number; page: number; pageSize: number; totalPages: number; }
export interface SaveEmployeeRequest { employeeNumber: string; fullName: string; departmentId: string; isActive: boolean; }
