using System.Runtime.CompilerServices;

// Grant the integration-test project visibility into internal members of this assembly.
// This is used solely by TestModelCustomizer to access CurrentFirmId for applying the
// tenant query filter to test-only entities (TenantTestRow).
[assembly: InternalsVisibleTo("Accounts.PracticeOperations.IntegrationTests")]
