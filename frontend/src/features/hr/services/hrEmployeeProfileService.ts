import { apiClient } from '../../../services/apiClient';
import type {
  ChangeEmployeeStatusRequest,
  EmployeeProfile,
  EmployeeReportingLine,
  UpdateEmployeeCompensationRequest,
  UpdateEmployeeContactRequest,
  UpdateEmployeeContractRequest,
  UpdateEmployeeEmergencyContactRequest,
  UpdateEmployeeEmploymentRequest,
  UpdateEmployeePersonalRequest,
} from '../types/employeeProfile';

export const hrEmployeeProfileService = {
  async getProfile(employeeId: string): Promise<EmployeeProfile> {
    const { data } = await apiClient.get<EmployeeProfile>(`/hr/employees/${employeeId}/profile`);
    return data;
  },

  async getReportingLine(employeeId: string): Promise<EmployeeReportingLine> {
    const { data } = await apiClient.get<EmployeeReportingLine>(`/hr/employees/${employeeId}/reporting-line`);
    return data;
  },

  async updatePersonal(employeeId: string, request: UpdateEmployeePersonalRequest): Promise<EmployeeProfile> {
    const { data } = await apiClient.put<EmployeeProfile>(`/hr/employees/${employeeId}/personal`, request);
    return data;
  },

  async updateContact(employeeId: string, request: UpdateEmployeeContactRequest): Promise<EmployeeProfile> {
    const { data } = await apiClient.put<EmployeeProfile>(`/hr/employees/${employeeId}/contact`, request);
    return data;
  },

  async updateEmployment(employeeId: string, request: UpdateEmployeeEmploymentRequest): Promise<EmployeeProfile> {
    const { data } = await apiClient.put<EmployeeProfile>(`/hr/employees/${employeeId}/employment`, request);
    return data;
  },

  async updateContract(employeeId: string, request: UpdateEmployeeContractRequest): Promise<EmployeeProfile> {
    const { data } = await apiClient.put<EmployeeProfile>(`/hr/employees/${employeeId}/contract`, request);
    return data;
  },

  async updateCompensation(employeeId: string, request: UpdateEmployeeCompensationRequest): Promise<EmployeeProfile> {
    const { data } = await apiClient.put<EmployeeProfile>(`/hr/employees/${employeeId}/compensation`, request);
    return data;
  },

  async updateEmergencyContact(employeeId: string, request: UpdateEmployeeEmergencyContactRequest): Promise<EmployeeProfile> {
    const { data } = await apiClient.put<EmployeeProfile>(`/hr/employees/${employeeId}/emergency-contact`, request);
    return data;
  },

  async changeStatus(employeeId: string, request: ChangeEmployeeStatusRequest): Promise<EmployeeProfile> {
    const { data } = await apiClient.patch<EmployeeProfile>(`/hr/employees/${employeeId}/status`, request);
    return data;
  },
};
