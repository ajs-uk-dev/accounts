using Accounts.PracticeOperations.Application.Abstractions;
using Accounts.PracticeOperations.Application.Firms.Register;
using Accounts.PracticeOperations.Domain.Firms;
using Accounts.PracticeOperations.Domain.Users;
using Accounts.SharedKernel.Time;
using FluentAssertions;
using NSubstitute;

namespace Accounts.PracticeOperations.UnitTests.Application;

public class RegisterFirmHandlerTests
{
    [Fact]
    public async Task Creates_firm_and_owner_user_when_slug_and_email_are_free()
    {
        var firms = Substitute.For<IFirmRepository>();
        var users = Substitute.For<IUserRepository>();
        var hasher = Substitute.For<IPasswordHasher>();
        var uow = Substitute.For<IUnitOfWork>();
        var clock = Substitute.For<IClock>();
        clock.UtcNow.Returns(new DateTimeOffset(2026, 5, 11, 9, 0, 0, TimeSpan.Zero));
        firms.GetBySlugAsync("acme").Returns((Firm?)null);
        users.GetByEmailAcrossFirmsAsync("alice@example.com").Returns((User?)null);
        hasher.Hash("super-secret-password").Returns("$hash$");

        var handler = new RegisterFirmHandler(firms, users, hasher, uow, clock);
        var result = await handler.Handle(
            new RegisterFirmCommand("Acme & Co", "acme", "alice@example.com", "super-secret-password"),
            CancellationToken.None);

        result.FirmId.Should().NotBeEmpty();
        result.OwnerUserId.Should().NotBeEmpty();
        await firms.Received(1).AddAsync(Arg.Any<Firm>());
        await users.Received(1).AddAsync(Arg.Is<User>(u => u.Role == Role.FirmOwner));
        await uow.Received(1).SaveChangesAsync();
    }

    [Fact]
    public async Task Fails_when_slug_already_taken()
    {
        var firms = Substitute.For<IFirmRepository>();
        firms.GetBySlugAsync("acme").Returns(Firm.Register("Acme", "acme", DateTimeOffset.UtcNow));
        var handler = new RegisterFirmHandler(
            firms, Substitute.For<IUserRepository>(),
            Substitute.For<IPasswordHasher>(), Substitute.For<IUnitOfWork>(),
            Substitute.For<IClock>());

        var act = () => handler.Handle(
            new RegisterFirmCommand("Acme", "acme", "x@y.com", "long-enough-pwd"),
            CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("*slug*taken*");
    }
}
