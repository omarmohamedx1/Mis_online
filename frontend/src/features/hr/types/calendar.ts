export type CalendarDayOfWeek = 0 | 1 | 2 | 3 | 4 | 5 | 6;

export interface WorkingDaySetting {
  dayOfWeek: CalendarDayOfWeek;
  isWorkingDay: boolean;
  startTime: string | null;
  endTime: string | null;
  breakMinutes: number;
  lateGraceMinutes: number;
  earlyLeaveGraceMinutes: number;
  minimumOvertimeMinutes: number;
}

export interface WorkingCalendar {
  id: string;
  name: string;
  timeZoneId: string;
  days: WorkingDaySetting[];
  createdAt: string;
  updatedAt: string | null;
}

export interface UpdateWorkingCalendarRequest {
  name: string;
  timeZoneId: string;
  days: WorkingDaySetting[];
}

export const calendarExceptionTypes = ['OfficialHoliday', 'CompanyHoliday', 'SpecialDay'] as const;
export type CalendarExceptionType = (typeof calendarExceptionTypes)[number];

export const calendarOverrideModes = ['NonWorkingDay', 'WorkingDay', 'CustomWorkingHours'] as const;
export type CalendarOverrideMode = (typeof calendarOverrideModes)[number];

export interface CalendarExceptionListItem {
  id: string;
  nameEnglish: string;
  nameArabic: string | null;
  date: string;
  type: CalendarExceptionType;
  overrideMode: CalendarOverrideMode;
  isActive: boolean;
}

export interface CalendarExceptionDetails extends CalendarExceptionListItem {
  startTime: string | null;
  endTime: string | null;
  breakMinutes: number | null;
  description: string | null;
  createdAt: string;
  updatedAt: string | null;
}

export interface PagedCalendarExceptions {
  items: CalendarExceptionListItem[];
  totalCount: number;
  page: number;
  pageSize: number;
  totalPages: number;
}

export interface CalendarExceptionQuery {
  page: number;
  pageSize: number;
  search?: string;
  dateFrom?: string;
  dateTo?: string;
  type?: CalendarExceptionType | '';
  isActive?: boolean | null;
}

export interface SaveCalendarExceptionRequest {
  nameEnglish: string;
  nameArabic: string | null;
  date: string;
  type: CalendarExceptionType;
  overrideMode: CalendarOverrideMode;
  startTime: string | null;
  endTime: string | null;
  breakMinutes: number | null;
  description: string | null;
  isActive: boolean;
}
