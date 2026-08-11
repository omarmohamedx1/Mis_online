namespace MIS.Domain.Constants;

public static class AttendanceValues
{
    public const string PresentStatus = "Present";
    public const string AbsentStatus = "Absent";
    public const string LateStatus = "Late";
    public const string LeaveStatus = "Leave";
    public const string HolidayStatus = "Holiday";
    public const string WeekendStatus = "Weekend";

    public const string ExcelImportSource = "ExcelImport";
    public const string ManualSource = "Manual";
    public const string DeviceIntegrationSource = "DeviceIntegration";
    public const string SystemProcessingSource = "SystemProcessing";

    public const string CheckInPunch = "CheckIn";
    public const string CheckOutPunch = "CheckOut";
    public const string UnknownPunch = "Unknown";

    public const string UploadedBatchStatus = "Uploaded";
    public const string PreviewReadyBatchStatus = "PreviewReady";
    public const string ConfirmedBatchStatus = "Confirmed";
    public const string FailedBatchStatus = "Failed";
    public const string CancelledBatchStatus = "Cancelled";

    public static string? NormalizeStatus(string? value) => value?.Trim().ToLowerInvariant() switch
    {
        "present" => PresentStatus,
        "absent" => AbsentStatus,
        "late" => LateStatus,
        "leave" => LeaveStatus,
        "holiday" => HolidayStatus,
        "weekend" => WeekendStatus,
        _ => null
    };

    public static string? NormalizeSource(string? value) => value?.Trim().ToLowerInvariant() switch
    {
        "excelimport" or "excel_import" or "excel import" => ExcelImportSource,
        "manual" => ManualSource,
        "deviceintegration" or "device_integration" or "device integration" => DeviceIntegrationSource,
        "systemprocessing" or "system_processing" or "system processing" => SystemProcessingSource,
        _ => null
    };

    public static string? NormalizePunchType(string? value) => value?.Trim().ToLowerInvariant() switch
    {
        "checkin" or "check_in" or "check in" or "in" => CheckInPunch,
        "checkout" or "check_out" or "check out" or "out" => CheckOutPunch,
        "unknown" or "" => UnknownPunch,
        _ => null
    };

    public static string? NormalizeBatchStatus(string? value) => value?.Trim().ToLowerInvariant() switch
    {
        "uploaded" => UploadedBatchStatus,
        "previewready" or "preview_ready" or "preview ready" => PreviewReadyBatchStatus,
        "confirmed" => ConfirmedBatchStatus,
        "failed" => FailedBatchStatus,
        "cancelled" or "canceled" => CancelledBatchStatus,
        _ => null
    };
}
