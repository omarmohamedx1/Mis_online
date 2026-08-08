namespace MIS.Application.Interfaces;

public enum PasswordVerificationStatus
{
    Failed = 0,
    Success = 1,
    SuccessRehashNeeded = 2
}
