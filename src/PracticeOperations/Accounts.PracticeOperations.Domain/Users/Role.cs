namespace Accounts.PracticeOperations.Domain.Users;

/// <summary>
/// Internal staff roles. Per spec §2.2. Client portal roles
/// (ClientPrimary/ClientStaff/ClientReadOnly) and MLRO/DPO overlays come in later sub-plans.
/// </summary>
public enum Role
{
    FirmOwner = 1,
    Partner = 2,
    Manager = 3,
    FeeEarner = 4,
    PracticeAdmin = 5,
}
