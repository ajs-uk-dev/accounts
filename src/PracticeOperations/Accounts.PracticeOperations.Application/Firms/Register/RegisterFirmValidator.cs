using Accounts.PracticeOperations.Domain.Users;
using FluentValidation;

namespace Accounts.PracticeOperations.Application.Firms.Register;

public sealed class RegisterFirmValidator : AbstractValidator<RegisterFirmCommand>
{
    public RegisterFirmValidator()
    {
        RuleFor(x => x.FirmName).NotEmpty().MaximumLength(200);
        RuleFor(x => x.FirmSlug).NotEmpty().MaximumLength(64).Matches("^[a-z0-9](?:[a-z0-9-]{0,62}[a-z0-9])?$");
        // Use the domain's own regex so the validator and EmailAddress.Create agree;
        // otherwise FluentValidation's default .EmailAddress() (AspNetCore mode) only
        // checks for '@' and lets values like "alice@localhost" reach the handler.
        RuleFor(x => x.OwnerEmail).NotEmpty().Matches(EmailAddress.RegexPattern)
            .WithMessage("'{PropertyName}' must be a valid email address.");
        RuleFor(x => x.OwnerPassword).NotEmpty().MinimumLength(12)
            .WithMessage("Password must be at least 12 characters.");
    }
}
