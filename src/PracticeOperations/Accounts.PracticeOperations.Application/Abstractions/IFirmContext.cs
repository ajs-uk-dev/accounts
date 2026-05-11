using Accounts.SharedKernel.Identity;

namespace Accounts.PracticeOperations.Application.Abstractions;

public interface IFirmContext
{
    /// <summary>FirmId of the current request, or null if unauthenticated/anonymous.</summary>
    FirmId? FirmId { get; }
    /// <summary>UserId of the current request, or null if unauthenticated/anonymous.</summary>
    UserId? UserId { get; }
    bool IsAuthenticated { get; }
}
