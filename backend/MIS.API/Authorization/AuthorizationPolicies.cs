namespace MIS.API.Authorization;

public static class AuthorizationPolicies
{
    public const string HrDepartment = "HrDepartment";
    public const string HrSensitiveData = "HrSensitiveData";
    public const string CollectionsAccess = "CollectionsAccess";
    public const string CollectionsSensitiveData = "CollectionsSensitiveData";
    public const string CollectionsAssignmentManage = "CollectionsAssignmentManage";
    public const string CollectionsPaymentApprove = "CollectionsPaymentApprove";
    public const string CollectionsAuditView = "CollectionsAuditView";
    public const string CollectionsImportManage = "CollectionsImportManage";
    public const string CollectionsConfigurationManage = "CollectionsConfigurationManage";
    public const string CollectionsReportExport = "CollectionsReportExport";
}
