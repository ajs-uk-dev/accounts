using Accounts.PracticeOperations.Domain.Firms;
using Accounts.SharedKernel.Identity;

namespace Accounts.PracticeOperations.Application.Abstractions;

public interface IFirmRepository
{
    Task<Firm?> GetAsync(FirmId id, CancellationToken ct = default);
    Task<Firm?> GetBySlugAsync(string slug, CancellationToken ct = default);
    Task AddAsync(Firm firm, CancellationToken ct = default);
}
