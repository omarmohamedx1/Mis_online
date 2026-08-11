using System.ComponentModel.DataAnnotations;

namespace MIS.Application.DTOs.Hr;

public static class HrCalendarExceptionTypes
{
    public const string OfficialHoliday = "OfficialHoliday";
    public const string CompanyHoliday = "CompanyHoliday";
    public const string SpecialDay = "SpecialDay";

    public static readonly IReadOnlyCollection<string> All =
    [
        OfficialHoliday,
        CompanyHoliday,
        SpecialDay
    ];
}

public static class HrCalendarOverrideModes
{
    public const string NonWorkingDay = "NonWorkingDay";
    public const string WorkingDay = "WorkingDay";
    public const string CustomWorkingHours = "CustomWorkingHours";

    public static readonly IReadOnlyCollection<string> All =
    [
        NonWorkingDay,
        WorkingDay,
        CustomWorkingHours
    ];
}

public sealed record WorkingCalendarDto(
    Guid Id,
    string Name,
    string TimeZoneId,
    IReadOnlyCollection<WorkingDaySettingDto> Days,
    DateTimeOffset CreatedAt,
    DateTimeOffset? UpdatedAt);

public sealed record WorkingDaySettingDto(
    DayOfWeek DayOfWeek,
    bool IsWorkingDay,
    TimeOnly? StartTime,
    TimeOnly? EndTime,
    int BreakMinutes,
    int LateGraceMinutes,
    int EarlyLeaveGraceMinutes,
    int MinimumOvertimeMinutes);

public sealed record CalendarExceptionListItemDto(
    Guid Id,
    string NameEnglish,
    string? NameArabic,
    DateOnly Date,
    string Type,
    string OverrideMode,
    bool IsActive);

public sealed record CalendarExceptionDetailsDto(
    Guid Id,
    string NameEnglish,
    string? NameArabic,
    DateOnly Date,
    string Type,
    string OverrideMode,
    TimeOnly? StartTime,
    TimeOnly? EndTime,
    int? BreakMinutes,
    string? Description,
    bool IsActive,
    DateTimeOffset CreatedAt,
    DateTimeOffset? UpdatedAt);

public sealed record PagedCalendarExceptionsDto(
    IReadOnlyCollection<CalendarExceptionListItemDto> Items,
    int TotalCount,
    int Page,
    int PageSize,
    int TotalPages);

public sealed class UpdateWorkingCalendarRequest
{
    [Required, StringLength(120, MinimumLength = 2)]
    public string Name { get; init; } = string.Empty;

    [Required, StringLength(100, MinimumLength = 1)]
    public string TimeZoneId { get; init; } = string.Empty;

    [Required, MinLength(7), MaxLength(7)]
    public IReadOnlyCollection<SaveWorkingDaySettingRequest> Days { get; init; } = [];
}

public sealed class SaveWorkingDaySettingRequest
{
    [EnumDataType(typeof(DayOfWeek))]
    public DayOfWeek DayOfWeek { get; init; }

    public bool IsWorkingDay { get; init; }

    public TimeOnly? StartTime { get; init; }

    public TimeOnly? EndTime { get; init; }

    [Range(0, 1440)]
    public int BreakMinutes { get; init; }

    [Range(0, 240)]
    public int LateGraceMinutes { get; init; }

    [Range(0, 240)]
    public int EarlyLeaveGraceMinutes { get; init; }

    [Range(0, 1440)]
    public int MinimumOvertimeMinutes { get; init; }
}

public sealed class CalendarExceptionFilterDto
{
    [Range(1, int.MaxValue)]
    public int Page { get; init; } = 1;

    [Range(1, 200)]
    public int PageSize { get; init; } = 20;

    [StringLength(160)]
    public string? Search { get; init; }

    public DateOnly? DateFrom { get; init; }

    public DateOnly? DateTo { get; init; }

    [StringLength(32)]
    public string? Type { get; init; }

    public bool? IsActive { get; init; }
}

public sealed class CreateCalendarExceptionRequest
{
    [Required, StringLength(120, MinimumLength = 2)]
    public string NameEnglish { get; init; } = string.Empty;

    [StringLength(120)]
    public string? NameArabic { get; init; }

    [Required]
    public DateOnly Date { get; init; }

    [Required, StringLength(32)]
    public string Type { get; init; } = HrCalendarExceptionTypes.OfficialHoliday;

    [Required, StringLength(32)]
    public string OverrideMode { get; init; } = HrCalendarOverrideModes.NonWorkingDay;

    public TimeOnly? StartTime { get; init; }

    public TimeOnly? EndTime { get; init; }

    [Range(0, 1440)]
    public int? BreakMinutes { get; init; }

    [StringLength(1000)]
    public string? Description { get; init; }

    public bool IsActive { get; init; } = true;
}

public sealed class UpdateCalendarExceptionRequest
{
    [Required, StringLength(120, MinimumLength = 2)]
    public string NameEnglish { get; init; } = string.Empty;

    [StringLength(120)]
    public string? NameArabic { get; init; }

    [Required]
    public DateOnly Date { get; init; }

    [Required, StringLength(32)]
    public string Type { get; init; } = HrCalendarExceptionTypes.OfficialHoliday;

    [Required, StringLength(32)]
    public string OverrideMode { get; init; } = HrCalendarOverrideModes.NonWorkingDay;

    public TimeOnly? StartTime { get; init; }

    public TimeOnly? EndTime { get; init; }

    [Range(0, 1440)]
    public int? BreakMinutes { get; init; }

    [StringLength(1000)]
    public string? Description { get; init; }

    public bool IsActive { get; init; } = true;
}

public sealed class SetCalendarExceptionActiveRequest
{
    public bool IsActive { get; init; }
}

public sealed class DeleteCalendarExceptionRequest
{
    [StringLength(500)]
    public string? Reason { get; init; }
}
