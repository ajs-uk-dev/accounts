using Accounts.SharedKernel.Domain;
using Accounts.SharedKernel.Identity;

// CA1707: '_Test' suffix is deliberate — this namespace holds test-only entities
// that exist purely to exercise infrastructure (the tenant query filter) until
// real aggregates arrive. The underscore makes the temporary nature obvious.
#pragma warning disable CA1707
namespace Accounts.PracticeOperations.Domain._Test;
#pragma warning restore CA1707

public class TenantTestRow : ITenantScopedEntity
{
    public Guid Id { get; private set; }
    public FirmId FirmId { get; private set; }
    public string Label { get; private set; } = string.Empty;

    private TenantTestRow() { }
    public TenantTestRow(FirmId firmId, string label)
    {
        Id = Guid.NewGuid();
        FirmId = firmId;
        Label = label;
    }
}
