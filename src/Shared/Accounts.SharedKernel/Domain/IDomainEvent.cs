using MediatR;

namespace Accounts.SharedKernel.Domain;

public interface IDomainEvent : INotification
{
    DateTimeOffset OccurredAt { get; }
}
