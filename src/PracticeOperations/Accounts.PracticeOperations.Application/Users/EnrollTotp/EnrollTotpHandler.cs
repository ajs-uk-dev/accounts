using Accounts.PracticeOperations.Application.Abstractions;
using Accounts.SharedKernel.Identity;
using Accounts.SharedKernel.Time;
using MediatR;

namespace Accounts.PracticeOperations.Application.Users.EnrollTotp;

public sealed class EnrollTotpHandler : IRequestHandler<EnrollTotpCommand, EnrollTotpResult>
{
    private readonly IUserRepository _users;
    private readonly ITotpService _totp;
    private readonly IUnitOfWork _uow;
    private readonly IClock _clock;

    public EnrollTotpHandler(IUserRepository users, ITotpService totp, IUnitOfWork uow, IClock clock)
    {
        _users = users; _totp = totp; _uow = uow; _clock = clock;
    }

    public async Task<EnrollTotpResult> Handle(EnrollTotpCommand cmd, CancellationToken cancellationToken)
    {
        var user = await _users.GetAsync(new UserId(cmd.UserId), cancellationToken)
            ?? throw new InvalidOperationException("User not found.");

        var secret = _totp.GenerateSecret();
        user.EnrollTotp(secret, _clock.UtcNow);
        await _uow.SaveChangesAsync(cancellationToken);

        var uri = _totp.BuildOtpAuthUri(secret, user.Email.Value, issuer: "Accounts");
        return new EnrollTotpResult(secret, uri);
    }
}
