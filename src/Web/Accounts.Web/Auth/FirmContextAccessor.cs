using Accounts.PracticeOperations.Application.Abstractions;
using Accounts.SharedKernel.Identity;

namespace Accounts.Web.Auth;

public sealed class FirmContextAccessor : IFirmContext
{
    public const string FirmIdClaim = "firm_id";
    private readonly IHttpContextAccessor _accessor;

    public FirmContextAccessor(IHttpContextAccessor accessor) => _accessor = accessor;

    private System.Security.Claims.ClaimsPrincipal? User =>
        _accessor.HttpContext?.User;

    public FirmId? FirmId
    {
        get
        {
            var value = User?.FindFirst(FirmIdClaim)?.Value;
            return Guid.TryParse(value, out var g) && g != Guid.Empty
                ? new FirmId(g) : null;
        }
    }

    public UserId? UserId
    {
        get
        {
            var value = User?.FindFirst("sub")?.Value
                ?? User?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            return Guid.TryParse(value, out var g) && g != Guid.Empty
                ? new SharedKernel.Identity.UserId(g) : null;
        }
    }

    public bool IsAuthenticated => User?.Identity?.IsAuthenticated == true;
}
