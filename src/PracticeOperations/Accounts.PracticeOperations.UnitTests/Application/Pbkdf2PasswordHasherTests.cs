using Accounts.PracticeOperations.Infrastructure.Auth;
using FluentAssertions;

namespace Accounts.PracticeOperations.UnitTests.Application;

public class Pbkdf2PasswordHasherTests
{
    [Fact]
    public void Round_trips_password()
    {
        var h = new Pbkdf2PasswordHasher();
        var hash = h.Hash("correct horse battery staple");
        h.Verify(hash, "correct horse battery staple").Should().BeTrue();
        h.Verify(hash, "wrong").Should().BeFalse();
    }
}
