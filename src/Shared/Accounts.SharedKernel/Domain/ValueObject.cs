namespace Accounts.SharedKernel.Domain;

public abstract class ValueObject
{
    protected abstract IEnumerable<object?> GetEqualityComponents();

    public override bool Equals(object? obj) =>
        obj is ValueObject vo
        && vo.GetType() == GetType()
        && GetEqualityComponents().SequenceEqual(vo.GetEqualityComponents());

    public override int GetHashCode() =>
        GetEqualityComponents()
            .Select(c => c?.GetHashCode() ?? 0)
            .Aggregate(0, (a, b) => HashCode.Combine(a, b));
}
