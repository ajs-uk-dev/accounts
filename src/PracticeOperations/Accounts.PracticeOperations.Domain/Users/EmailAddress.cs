using System.Text.RegularExpressions;
using Accounts.SharedKernel.Domain;
using Accounts.SharedKernel.Results;

namespace Accounts.PracticeOperations.Domain.Users;

public sealed class EmailAddress : ValueObject
{
    // RFC-5322 simplified: one @, non-empty local, dot in domain.
    // Exposed so the FluentValidation rule and the value object stay in lockstep.
    public const string RegexPattern = @"^[^@\s]+@[^@\s]+\.[^@\s]+$";

    private static readonly Regex Pattern = new(RegexPattern, RegexOptions.Compiled);

    public string Value { get; }

    private EmailAddress(string value) => Value = value;

    public static Result<EmailAddress> Create(string? raw)
    {
        var trimmed = raw?.Trim().ToLowerInvariant();
        if (string.IsNullOrEmpty(trimmed) || !Pattern.IsMatch(trimmed))
            return Result<EmailAddress>.Failure("Email.Invalid", $"'{raw}' is not a valid email address.");
        return Result<EmailAddress>.Success(new EmailAddress(trimmed));
    }

    protected override IEnumerable<object?> GetEqualityComponents() { yield return Value; }
    public override string ToString() => Value;
}
