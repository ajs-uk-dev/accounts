namespace Accounts.SharedKernel.Results;

public sealed record DomainError(string Code, string Message);

#pragma warning disable CA1000 // Static factory methods are idiomatic for the Result<T> pattern.
public readonly record struct Result<T>
{
    public T? Value { get; }
    public DomainError? Error { get; }
    public bool IsSuccess => Error is null;
    public bool IsFailure => Error is not null;

    private Result(T? value, DomainError? error)
    {
        Value = value;
        Error = error;
    }

    public static Result<T> Success(T value) => new(value, null);
    public static Result<T> Failure(DomainError error) => new(default, error);
    public static Result<T> Failure(string code, string message) =>
        new(default, new DomainError(code, message));
}
#pragma warning restore CA1000
