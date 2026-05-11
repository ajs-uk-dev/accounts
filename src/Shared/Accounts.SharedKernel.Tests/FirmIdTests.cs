using Accounts.SharedKernel.Identity;
using FluentAssertions;

namespace Accounts.SharedKernel.Tests;

public class FirmIdTests
{
    [Fact]
    public void New_ReturnsDistinctValues()
    {
        var a = FirmId.New();
        var b = FirmId.New();
        a.Should().NotBe(b);
        a.Value.Should().NotBeEmpty();
    }

    [Fact]
    public void Equality_IsByValue()
    {
        var guid = Guid.NewGuid();
        var a = new FirmId(guid);
        var b = new FirmId(guid);
        a.Should().Be(b);
        (a == b).Should().BeTrue();
    }

    [Fact]
    public void Parse_RoundTripsThroughString()
    {
        var original = FirmId.New();
        var parsed = FirmId.Parse(original.ToString());
        parsed.Should().Be(original);
    }

    [Fact]
    public void Empty_GuidIsRejected()
    {
        var act = () => new FirmId(Guid.Empty);
        act.Should().Throw<ArgumentException>();
    }
}
