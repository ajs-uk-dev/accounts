using Accounts.PracticeOperations.Application.Abstractions;
using Accounts.PracticeOperations.Domain.Firms;
using Accounts.PracticeOperations.Domain.Users;
using Accounts.SharedKernel.Time;
using MediatR;

namespace Accounts.PracticeOperations.Application.Firms.Register;

public sealed class RegisterFirmHandler : IRequestHandler<RegisterFirmCommand, RegisterFirmResult>
{
    private readonly IFirmRepository _firms;
    private readonly IUserRepository _users;
    private readonly IPasswordHasher _hasher;
    private readonly IUnitOfWork _uow;
    private readonly IClock _clock;

    public RegisterFirmHandler(
        IFirmRepository firms, IUserRepository users,
        IPasswordHasher hasher, IUnitOfWork uow, IClock clock)
    {
        _firms = firms; _users = users; _hasher = hasher; _uow = uow; _clock = clock;
    }

    public async Task<RegisterFirmResult> Handle(RegisterFirmCommand cmd, CancellationToken cancellationToken)
    {
        var existing = await _firms.GetBySlugAsync(cmd.FirmSlug, cancellationToken);
        if (existing is not null)
            throw new InvalidOperationException($"Firm slug '{cmd.FirmSlug}' is already taken.");

        var email = EmailAddress.Create(cmd.OwnerEmail);
        if (email.IsFailure)
            throw new InvalidOperationException(email.Error!.Message);

        var existingUser = await _users.GetByEmailAcrossFirmsAsync(email.Value!.Value, cancellationToken);
        if (existingUser is not null)
            throw new InvalidOperationException($"Email '{email.Value.Value}' is already registered.");

        var now = _clock.UtcNow;
        var firm = Firm.Register(cmd.FirmName, cmd.FirmSlug, now);
        var owner = User.Register(firm.Id, email.Value, _hasher.Hash(cmd.OwnerPassword), Role.FirmOwner, now);

        await _firms.AddAsync(firm, cancellationToken);
        await _users.AddAsync(owner, cancellationToken);
        await _uow.SaveChangesAsync(cancellationToken);

        return new RegisterFirmResult(firm.Id.Value, owner.Id.Value);
    }
}
