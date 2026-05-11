using Accounts.PracticeOperations.Domain.Users.Events;
using Accounts.SharedKernel.Domain;
using Accounts.SharedKernel.Identity;

namespace Accounts.PracticeOperations.Domain.Users;

public sealed class User : AggregateRoot<UserId>, ITenantScopedEntity
{
    public FirmId FirmId { get; private set; }
    public EmailAddress Email { get; private set; } = null!;
    public string PasswordHash { get; private set; } = string.Empty;
    public Role Role { get; private set; }
    public UserStatus Status { get; private set; }
    public bool TotpEnrolled { get; private set; }
    public string? TotpSecret { get; private set; }
    public DateTimeOffset? LastSignInAt { get; private set; }
    public int FailedSignInAttempts { get; private set; }

    private User() { }

    public static User Register(FirmId firmId, EmailAddress email, string passwordHash, Role role, DateTimeOffset now)
    {
        var user = new User();
        user.AssignIdentity(UserId.New(), now);
        user.FirmId = firmId;
        user.Email = email;
        user.PasswordHash = passwordHash;
        user.Role = role;
        user.Status = UserStatus.PendingVerification;
        user.Raise(new UserRegistered(user.Id, firmId, email.Value, role, now));
        return user;
    }

    public void Activate(DateTimeOffset now)
    {
        if (Status != UserStatus.PendingVerification)
            throw new InvalidOperationException($"Cannot activate user in status {Status}.");
        Status = UserStatus.Active;
        Touch(now);
    }

    public void EnrollTotp(string totpSecret, DateTimeOffset now)
    {
        if (string.IsNullOrWhiteSpace(totpSecret))
            throw new ArgumentException("TOTP secret required.", nameof(totpSecret));
        TotpSecret = totpSecret;
        TotpEnrolled = true;
        Touch(now);
    }

    public void RecordSuccessfulSignIn(DateTimeOffset now)
    {
        LastSignInAt = now;
        FailedSignInAttempts = 0;
        Touch(now);
    }

    public void RecordFailedSignIn(DateTimeOffset now)
    {
        FailedSignInAttempts++;
        Touch(now);
    }
}
