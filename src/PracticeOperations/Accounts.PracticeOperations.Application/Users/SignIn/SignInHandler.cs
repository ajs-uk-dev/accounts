using Accounts.PracticeOperations.Application.Abstractions;
using Accounts.PracticeOperations.Domain.Users;
using Accounts.SharedKernel.Time;
using MediatR;

namespace Accounts.PracticeOperations.Application.Users.SignIn;

public sealed class SignInHandler : IRequestHandler<SignInCommand, SignInResult>
{
    private readonly IUserRepository _users;
    private readonly IPasswordHasher _hasher;
    private readonly ITotpService _totp;
    private readonly IJwtIssuer _jwt;
    private readonly IUnitOfWork _uow;
    private readonly IClock _clock;

    public SignInHandler(
        IUserRepository users, IPasswordHasher hasher, ITotpService totp,
        IJwtIssuer jwt, IUnitOfWork uow, IClock clock)
    {
        _users = users; _hasher = hasher; _totp = totp;
        _jwt = jwt; _uow = uow; _clock = clock;
    }

    public async Task<SignInResult> Handle(SignInCommand cmd, CancellationToken cancellationToken)
    {
        var user = await _users.GetByEmailAcrossFirmsAsync(cmd.Email, cancellationToken);
        if (user is null || !_hasher.Verify(user.PasswordHash, cmd.Password))
            throw new UnauthorizedAccessException("Invalid email or password.");

        if (user.Status != UserStatus.Active && user.Status != UserStatus.PendingVerification)
            throw new UnauthorizedAccessException("Account is not active.");

        if (user.TotpEnrolled)
        {
            if (string.IsNullOrEmpty(cmd.TotpCode))
                return new SignInResult(string.Empty, default, TotpRequired: true);
            if (!_totp.Verify(user.TotpSecret!, cmd.TotpCode))
            {
                user.RecordFailedSignIn(_clock.UtcNow);
                await _uow.SaveChangesAsync(cancellationToken);
                throw new UnauthorizedAccessException("Invalid TOTP code.");
            }
        }

        // MFA grace: a freshly-registered user is PendingVerification with no TOTP - auto-activate on first sign-in.
        // TOTP enrolment will be required on the next request via a downstream "totp_required" claim flow.
        if (user.Status == UserStatus.PendingVerification)
            user.Activate(_clock.UtcNow);

        user.RecordSuccessfulSignIn(_clock.UtcNow);
        await _uow.SaveChangesAsync(cancellationToken);

        var (token, expires) = _jwt.Issue(user.FirmId, user.Id, user.Role);
        return new SignInResult(token, expires, TotpRequired: false);
    }
}
