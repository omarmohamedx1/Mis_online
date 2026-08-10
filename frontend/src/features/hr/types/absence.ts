export type AbsenceStatus = 'Pending' | 'Excused' | 'Unexcused';
export interface AbsenceListItem { id: string; employeeId: string; employeeNumber: string; employeeName: string; departmentId: string; departmentName: string; absenceDate: string; type: 'Absent'; status: AbsenceStatus; }
export interface AbsenceDetails extends AbsenceListItem { reason: string | null; notes: string | null; attendanceSource: 'Manual'; createdAt: string; updatedAt: string | null; }
export interface PagedAbsences { items: AbsenceListItem[]; totalCount: number; page: number; pageSize: number; totalPages: number; }
export interface SaveAbsenceRequest { employeeId: string; absenceDate: string; type: 'Absent'; reason: string; status: AbsenceStatus; notes: string; attendanceSource: 'Manual'; }
