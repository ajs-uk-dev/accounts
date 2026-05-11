using Accounts.PracticeOperations.Application.Behaviors;
using Accounts.PracticeOperations.Domain.Audit;
using MediatR;

namespace Accounts.PracticeOperations.Application.Users.EnrollTotp;

public sealed record EnrollTotpCommand(Guid UserId)
    : IRequest<EnrollTotpResult>, IAuditedCommand
{
    public AuditAction Action => AuditAction.UserTotpEnrolled;
    public string EntityType => "User";
    public string EntityId => UserId.ToString();
}

public sealed record EnrollTotpResult(string Secret, string OtpAuthUri);
