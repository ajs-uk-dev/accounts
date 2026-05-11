namespace Accounts.SharedKernel.Identity;

public readonly record struct UserId
{
    public Guid Value { get; }

    public UserId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("UserId cannot be empty", nameof(value));
        Value = value;
    }

    public static UserId New() => new(Guid.NewGuid());
    public static UserId Parse(string s) => new(Guid.Parse(s));
    public override string ToString() => Value.ToString();
}
