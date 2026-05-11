namespace Accounts.SharedKernel.Exceptions;

/// <summary>
/// Thrown by application/domain code when a request cannot be completed because
/// it conflicts with the current state of a uniquely-constrained resource
/// (e.g. duplicate slug, duplicate email). Maps to HTTP 409 at the API boundary.
/// </summary>
public sealed class ConflictException : Exception
{
    public ConflictException(string message) : base(message) { }
}
