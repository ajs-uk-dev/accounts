using Accounts.PracticeOperations.Application.Abstractions;
using MediatR;

namespace Accounts.PracticeOperations.Application.Behaviors;

public sealed class AuditingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private readonly IAuditWriter _audit;
    public AuditingBehavior(IAuditWriter audit) => _audit = audit;

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        var response = await next();

        if (request is IAuditedCommand cmd)
        {
            await _audit.RecordAsync(cmd.Action, cmd.EntityType, cmd.EntityId, cmd.Payload, cancellationToken);
        }

        return response;
    }
}
