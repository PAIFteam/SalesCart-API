using System.Security.Claims;

namespace SalesCart.API.Extensions;

public static class JwtClaimsExtensions
{
    public static bool TryGetUserId(this ClaimsPrincipal user, out int userId)
    {
        userId = default;

        var raw = user.FindFirstValue(ClaimTypes.NameIdentifier)
                  ?? user.FindFirstValue("sub")
                  ?? user.FindFirstValue("id")
                  ?? user.FindFirstValue("userId");

        return int.TryParse(raw, out userId);
    }
}
