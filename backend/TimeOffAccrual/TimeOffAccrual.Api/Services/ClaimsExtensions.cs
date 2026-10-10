using System.Security.Claims;

namespace TimeOffAccrual.Api.Services;

public static class ClaimsExtensions
{
    public static int GetUserId(this ClaimsPrincipal user)
    {
        var value = user.FindFirstValue(ClaimTypes.NameIdentifier);
        return int.TryParse(value, out var id)
            ? id
            : throw new UnauthorizedAccessException("Missing or invalid user id claim.");
    }
}