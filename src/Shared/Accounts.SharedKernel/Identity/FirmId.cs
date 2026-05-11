namespace Accounts.SharedKernel.Identity;

public readonly record struct FirmId
{
    public Guid Value { get; }

    public FirmId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("FirmId cannot be empty", nameof(value));
        Value = value;
    }

    public static FirmId New() => new(Guid.NewGuid());
    public static FirmId Parse(string s) => new(Guid.Parse(s));
    public override string ToString() => Value.ToString();
}
