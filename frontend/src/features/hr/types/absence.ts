export type AbsenceStatus = 'Pending' | 'Excused' | 'Unexcused';
export type PayrollImpactStatus = 'NotApplicable' | 'PendingReview' | 'Approved' | 'Excluded';
export interface AbsenceListItem { id: string; employeeId: string; employeeNumber: string; employeeName: string; departmentId: string; departmentName: string; absenceDate: string; type: 'Absent'; status: AbsenceStatus; suggestedDeductionAmount: number; approvedDeductionAmount: number | null; payrollImpactStatus: PayrollImpactStatus; }
export interface AbsenceDetails extends AbsenceListItem { reason: string | null; notes: string | null; attendanceSource: 'Manual'; payrollNotes: string | null; payrollReviewedByUsername: string | null; payrollReviewedAt: string | null; createdAt: string; updatedAt: string | null; }
export interface PagedAbsences { items: AbsenceListItem[]; totalCount: number; page: number; pageSize: number; totalPages: number; }
export interface SaveAbsenceRequest { employeeId: string; absenceDate: string; type: 'Absent'; reason: string; status: AbsenceStatus; notes: string; attendanceSource: 'Manual'; }
export interface ReviewAbsencePayrollImpactRequest { decision: 'Approve' | 'Exclude'; approvedDeductionAmount: number | null; notes: string; }
