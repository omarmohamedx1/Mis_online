namespace MIS.Domain.Constants;

public static class CalendarValues
{
    public const string OfficialHolidayType = "OfficialHoliday";
    public const string CompanyHolidayType = "CompanyHoliday";
    public const string SpecialDayType = "SpecialDay";

    public const string NonWorkingDayOverride = "NonWorkingDay";
    public const string WorkingDayOverride = "WorkingDay";
    public const string CustomWorkingHoursOverride = "CustomWorkingHours";

    public static string? NormalizeExceptionType(string? value) => value?.Trim().ToLowerInvariant() switch
    {
        "officialholiday" or "official_holiday" or "official holiday" => OfficialHolidayType,
        "companyholiday" or "company_holiday" or "company holiday" => CompanyHolidayType,
        "specialday" or "special_day" or "special day" => SpecialDayType,
        _ => null
    };

    public static string? NormalizeOverrideMode(string? value) => value?.Trim().ToLowerInvariant() switch
    {
        "nonworkingday" or "non_working_day" or "non-working day" => NonWorkingDayOverride,
        "workingday" or "working_day" or "working day" => WorkingDayOverride,
        "customworkinghours" or "custom_working_hours" or "custom working hours" => CustomWorkingHoursOverride,
        _ => null
    };
}
