using Accounts.SharedKernel.Results;
using FluentAssertions;

namespace Accounts.SharedKernel.Tests;

public class ResultTests
{
    [Fact]
    public void Success_HasValueAndNoError()
    {
        var r = Result<int>.Success(42);
        r.IsSuccess.Should().BeTrue();
        r.Value.Should().Be(42);
        r.Error.Should().BeNull();
    }

    [Fact]
    public void Failure_HasErrorAndNoValue()
    {
        var r = Result<int>.Failure("E1", "oops");
        r.IsFailure.Should().BeTrue();
        r.Error.Should().Be(new DomainError("E1", "oops"));
    }
}
