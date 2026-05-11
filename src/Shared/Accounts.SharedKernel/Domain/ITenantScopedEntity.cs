using Accounts.SharedKernel.Identity;

namespace Accounts.SharedKernel.Domain;

public interface ITenantScopedEntity
{
    FirmId FirmId { get; }
}
