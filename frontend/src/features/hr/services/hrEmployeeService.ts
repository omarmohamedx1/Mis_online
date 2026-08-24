import { apiClient } from '../../../services/apiClient';
import type { DepartmentOption, EmployeeDetails, PagedEmployees, SaveEmployeeRequest } from '../types/employee';

export interface EmployeeQuery { page: number; pageSize: number; search: string; departmentId: string; status: string; role?: string; archived?: boolean; includeInactive?: boolean; }

export const hrEmployeeService = {
  async getEmployees(query: EmployeeQuery): Promise<PagedEmployees> {
    const { data } = await apiClient.get<PagedEmployees>('/hr/employees', { params: { page: query.page, pageSize: query.pageSize, search: query.search || undefined, departmentId: query.departmentId || undefined, status: query.includeInactive ? 'all' : query.status, role: query.role || undefined, archived: query.archived ?? false } });
    return data;
  },
  async getEmployee(id: string): Promise<EmployeeDetails> { const { data } = await apiClient.get<EmployeeDetails>(`/hr/employees/${id}`); return data; },
  async getDepartments(): Promise<DepartmentOption[]> { const { data } = await apiClient.get<DepartmentOption[]>('/hr/departments'); return data; },
  async createEmployee(request: SaveEmployeeRequest): Promise<EmployeeDetails> { const { data } = await apiClient.post<EmployeeDetails>('/hr/employees', request); return data; },
  async updateEmployee(id: string, request: SaveEmployeeRequest): Promise<EmployeeDetails> { const { data } = await apiClient.put<EmployeeDetails>(`/hr/employees/${id}`, request); return data; },
  async archiveEmployee(id: string, reason: string): Promise<EmployeeDetails> { const { data } = await apiClient.post<EmployeeDetails>(`/hr/employees/${id}/archive`, { reason }); return data; },
  async restoreEmployee(id: string): Promise<EmployeeDetails> { const { data } = await apiClient.post<EmployeeDetails>(`/hr/employees/${id}/restore`); return data; },
};
