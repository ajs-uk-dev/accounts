using MediatR;

namespace Accounts.PracticeOperations.Application.Users.SignIn;

public sealed record SignInCommand(
    string Email,
    string Password,
    string? TotpCode) : IRequest<SignInResult>;

public sealed record SignInResult(string AccessToken, DateTimeOffset ExpiresAt, bool TotpRequired);
