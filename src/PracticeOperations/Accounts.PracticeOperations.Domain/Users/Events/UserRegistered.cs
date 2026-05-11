using Accounts.SharedKernel.Domain;
using Accounts.SharedKernel.Identity;

namespace Accounts.PracticeOperations.Domain.Users.Events;

public sealed record UserRegistered(
    UserId UserId, FirmId FirmId, string Email, Role Role, DateTimeOffset OccurredAt) : IDomainEvent;
