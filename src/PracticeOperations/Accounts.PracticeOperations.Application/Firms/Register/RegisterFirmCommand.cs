using MediatR;

namespace Accounts.PracticeOperations.Application.Firms.Register;

public sealed record RegisterFirmCommand(
    string FirmName,
    string FirmSlug,
    string OwnerEmail,
    string OwnerPassword) : IRequest<RegisterFirmResult>;

public sealed record RegisterFirmResult(Guid FirmId, Guid OwnerUserId);
