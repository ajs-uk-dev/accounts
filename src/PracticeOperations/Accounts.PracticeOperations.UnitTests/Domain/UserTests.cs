using Accounts.PracticeOperations.Domain.Users;
using Accounts.PracticeOperations.Domain.Users.Events;
using Accounts.SharedKernel.Identity;
using FluentAssertions;

namespace Accounts.PracticeOperations.UnitTests.Domain;

public class UserTests
{
    [Fact]
    public void Register_creates_user_in_PendingVerification_with_event()
    {
        var firm = FirmId.New();
        var email = EmailAddress.Create("alice@example.com").Value!;
        var user = User.Register(firm, email, "hash-of-password", Role.FirmOwner, DateTimeOffset.UtcNow);

        user.FirmId.Should().Be(firm);
        user.Email.Should().Be(email);
        user.PasswordHash.Should().Be("hash-of-password");
        user.Role.Should().Be(Role.FirmOwner);
        user.Status.Should().Be(UserStatus.PendingVerification);
        user.TotpEnrolled.Should().BeFalse();
        user.DomainEvents.Should().ContainSingle(e => e is UserRegistered);
    }

    [Fact]
    public void Activate_moves_PendingVerification_to_Active()
    {
        var user = NewUser();
        user.Activate(DateTimeOffset.UtcNow);
        user.Status.Should().Be(UserStatus.Active);
    }

    [Fact]
    public void Activate_throws_if_not_PendingVerification()
    {
        var user = NewUser();
        user.Activate(DateTimeOffset.UtcNow);
        var act = () => user.Activate(DateTimeOffset.UtcNow);
        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void EnrollTotp_sets_secret_and_flag()
    {
        var user = NewUser();
        user.EnrollTotp("BASE32SECRETXXXX", DateTimeOffset.UtcNow);
        user.TotpEnrolled.Should().BeTrue();
        user.TotpSecret.Should().Be("BASE32SECRETXXXX");
    }

    private static User NewUser() => User.Register(
        FirmId.New(),
        EmailAddress.Create("alice@example.com").Value!,
        "hash", Role.FirmOwner, DateTimeOffset.UtcNow);
}
