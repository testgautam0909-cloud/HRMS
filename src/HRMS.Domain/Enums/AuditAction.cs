namespace HRMS.Domain.Enums;

public enum AuditAction
{
    Created = 1,
    Updated = 2,
    Deleted = 3,
    Approved = 4,
    Rejected = 5,
    Locked = 6,
    Overridden = 7,
    LoginSuccess = 8,
    LoginFailure = 9,
    PasswordChanged = 10,
    PasswordReset = 11
}
