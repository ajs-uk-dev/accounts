using Accounts.PracticeOperations.Domain.Firms;
using Accounts.PracticeOperations.Domain.Firms.Events;
using FluentAssertions;

namespace Accounts.PracticeOperations.UnitTests.Domain;

public class FirmTests
{
    [Fact]
    public void Register_creates_firm_in_Trial_status_and_raises_event()
    {
        var now = DateTimeOffset.UtcNow;
        var firm = Firm.Register("Smith & Co", "smith-co", now);

        firm.Name.Should().Be("Smith & Co");
        firm.Slug.Should().Be("smith-co");
        firm.Status.Should().Be(FirmStatus.Trial);
        firm.CreatedAt.Should().Be(now);
        firm.DomainEvents.Should().ContainSingle(e => e is FirmRegistered);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Register_rejects_blank_name(string name)
    {
        var act = () => Firm.Register(name, "ok-slug", DateTimeOffset.UtcNow);
        act.Should().Throw<ArgumentException>().WithMessage("*name*");
    }

    [Theory]
    [InlineData("UPPER")]                 // must be lowercased / kebab
    [InlineData("with spaces")]
    [InlineData("special!chars")]
    public void Register_rejects_invalid_slug(string slug)
    {
        var act = () => Firm.Register("Valid", slug, DateTimeOffset.UtcNow);
        act.Should().Throw<ArgumentException>().WithMessage("*slug*");
    }

    [Fact]
    public void Activate_moves_from_Trial_to_Active()
    {
        var firm = Firm.Register("Acme", "acme", DateTimeOffset.UtcNow);
        firm.Activate(DateTimeOffset.UtcNow);
        firm.Status.Should().Be(FirmStatus.Active);
    }
}
