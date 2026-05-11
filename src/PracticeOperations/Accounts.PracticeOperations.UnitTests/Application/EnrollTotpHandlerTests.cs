using Accounts.PracticeOperations.Application.Abstractions;
using Accounts.PracticeOperations.Application.Users.EnrollTotp;
using Accounts.PracticeOperations.Domain.Users;
using Accounts.SharedKernel.Identity;
using Accounts.SharedKernel.Time;
using FluentAssertions;
using NSubstitute;

namespace Accounts.PracticeOperations.UnitTests.Application;

public class EnrollTotpHandlerTests
{
    [Fact]
    public async Task Enrolls_user_and_returns_secret_and_uri()
    {
        var users = Substitute.For<IUserRepository>();
        var totp = Substitute.For<ITotpService>();
        var uow = Substitute.For<IUnitOfWork>();
        var clock = Substitute.For<IClock>();
        clock.UtcNow.Returns(new DateTimeOffset(2026, 5, 11, 10, 0, 0, TimeSpan.Zero));

        var userId = new UserId(Guid.NewGuid());
        var firmId = new FirmId(Guid.NewGuid());
        var existing = User.Register(
            firmId,
            EmailAddress.Create("alice@example.com").Value!,
            "$hash$",
            Role.FirmOwner,
            DateTimeOffset.UtcNow);
        // The factory generates its own UserId, but the handler will look up by the cmd's UserId
        // via the repo substitute. Wire the mock to return the existing instance.
        users.GetAsync(Arg.Any<UserId>()).Returns(existing);
        totp.GenerateSecret().Returns("JBSWY3DPEHPK3PXP");
        totp.BuildOtpAuthUri("JBSWY3DPEHPK3PXP", "alice@example.com", "Accounts").Returns("otpauth://x");

        var handler = new EnrollTotpHandler(users, totp, uow, clock);
        var result = await handler.Handle(new EnrollTotpCommand(userId.Value), CancellationToken.None);

        result.Secret.Should().Be("JBSWY3DPEHPK3PXP");
        result.OtpAuthUri.Should().Be("otpauth://x");
        existing.TotpEnrolled.Should().BeTrue();
        existing.TotpSecret.Should().Be("JBSWY3DPEHPK3PXP");
        await uow.Received(1).SaveChangesAsync();
    }

    [Fact]
    public async Task Throws_when_user_not_found()
    {
        var users = Substitute.For<IUserRepository>();
        var uow = Substitute.For<IUnitOfWork>();
        users.GetAsync(Arg.Any<UserId>()).Returns((User?)null);

        var handler = new EnrollTotpHandler(
            users, Substitute.For<ITotpService>(), uow, Substitute.For<IClock>());

        var act = () => handler.Handle(new EnrollTotpCommand(Guid.NewGuid()), CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("*User not found*");
        await uow.DidNotReceive().SaveChangesAsync();
    }
}
