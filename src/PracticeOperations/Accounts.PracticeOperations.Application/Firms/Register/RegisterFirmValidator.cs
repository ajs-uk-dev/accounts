using FluentValidation;

namespace Accounts.PracticeOperations.Application.Firms.Register;

public sealed class RegisterFirmValidator : AbstractValidator<RegisterFirmCommand>
{
    public RegisterFirmValidator()
    {
        RuleFor(x => x.FirmName).NotEmpty().MaximumLength(200);
        RuleFor(x => x.FirmSlug).NotEmpty().MaximumLength(64).Matches("^[a-z0-9](?:[a-z0-9-]{0,62}[a-z0-9])?$");
        RuleFor(x => x.OwnerEmail).NotEmpty().EmailAddress();
        RuleFor(x => x.OwnerPassword).NotEmpty().MinimumLength(12)
            .WithMessage("Password must be at least 12 characters.");
    }
}
