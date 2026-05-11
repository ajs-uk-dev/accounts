using System.Text.RegularExpressions;
using Accounts.PracticeOperations.Domain.Firms.Events;
using Accounts.SharedKernel.Domain;
using Accounts.SharedKernel.Identity;

namespace Accounts.PracticeOperations.Domain.Firms;

public sealed class Firm : AggregateRoot<FirmId>
{
    private static readonly Regex SlugPattern = new(@"^[a-z0-9](?:[a-z0-9-]{0,62}[a-z0-9])?$", RegexOptions.Compiled);

    public string Name { get; private set; } = string.Empty;
    public string Slug { get; private set; } = string.Empty;
    public FirmStatus Status { get; private set; }

    private Firm() { }

    public static Firm Register(string name, string slug, DateTimeOffset now)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Firm name must not be blank.", nameof(name));
        if (string.IsNullOrWhiteSpace(slug) || !SlugPattern.IsMatch(slug))
            throw new ArgumentException("Firm slug must be lowercase, kebab-case, 1-64 chars.", nameof(slug));

        var firm = new Firm();
        firm.AssignIdentity(FirmId.New(), now);
        firm.Name = name.Trim();
        firm.Slug = slug;
        firm.Status = FirmStatus.Trial;
        firm.Raise(new FirmRegistered(firm.Id, firm.Name, now));
        return firm;
    }

    public void Activate(DateTimeOffset now)
    {
        if (Status != FirmStatus.Trial)
            throw new InvalidOperationException($"Cannot activate firm in status {Status}.");
        Status = FirmStatus.Active;
        Touch(now);
    }
}
