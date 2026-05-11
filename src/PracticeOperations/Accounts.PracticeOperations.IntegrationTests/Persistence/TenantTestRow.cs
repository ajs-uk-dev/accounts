using Accounts.SharedKernel.Domain;
using Accounts.SharedKernel.Identity;

namespace Accounts.PracticeOperations.IntegrationTests.Persistence;

/// <summary>
/// Test-only entity used to exercise the tenant query filter in integration tests.
/// Lives in the test project to keep the production Domain assembly clean.
/// </summary>
internal sealed class TenantTestRow : ITenantScopedEntity
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
