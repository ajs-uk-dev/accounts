namespace Accounts.PracticeOperations.Domain.Audit;

public enum AuditAction
{
    Unknown = 0,

    // Firm lifecycle
    FirmRegistered = 100,

    // User lifecycle / auth
    UserRegistered = 200,
    UserSignedIn = 201,
    UserSignInFailed = 202,
    UserSignedOut = 203,
    UserTotpEnrolled = 204,
    UserRoleChanged = 205,
}
