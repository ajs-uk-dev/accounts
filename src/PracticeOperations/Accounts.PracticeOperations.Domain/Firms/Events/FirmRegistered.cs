using Accounts.SharedKernel.Domain;
using Accounts.SharedKernel.Identity;

namespace Accounts.PracticeOperations.Domain.Firms.Events;

public sealed record FirmRegistered(FirmId FirmId, string Name, DateTimeOffset OccurredAt) : IDomainEvent;
