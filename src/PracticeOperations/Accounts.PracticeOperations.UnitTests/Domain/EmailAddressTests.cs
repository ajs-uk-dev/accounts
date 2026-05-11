using Accounts.PracticeOperations.Domain.Users;
using FluentAssertions;

namespace Accounts.PracticeOperations.UnitTests.Domain;

public class EmailAddressTests
{
    [Theory]
    [InlineData("alice@example.com")]
    [InlineData("a.b+tag@example.co.uk")]
    public void Valid_email_is_accepted_and_lowercased(string input)
    {
        var e = EmailAddress.Create(input);
        e.IsSuccess.Should().BeTrue();
        e.Value!.Value.Should().Be(input.ToLowerInvariant());
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("not-an-email")]
    [InlineData("@example.com")]
    [InlineData("alice@")]
    public void Invalid_email_returns_failure(string input)
    {
        var e = EmailAddress.Create(input);
        e.IsFailure.Should().BeTrue();
        e.Error!.Code.Should().Be("Email.Invalid");
    }

    [Fact]
    public void Equality_is_case_insensitive_via_normalization()
    {
        var a = EmailAddress.Create("Alice@Example.com").Value!;
        var b = EmailAddress.Create("alice@example.COM").Value!;
        a.Should().Be(b);
    }
}
