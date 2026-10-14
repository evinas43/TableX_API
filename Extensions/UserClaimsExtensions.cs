using System.Security.Claims;
using TableX_API.Lib.Consts;
using TableX_API.Models;

namespace TableX_API.Extensions;

public static class UserClaimsExtensions
{
    public static int GetUserId(this ClaimsPrincipal user)
        => int.TryParse(user.FindFirstValue(ClaimTypes.NameIdentifier), out var id)
            ? id
            : throw new UnauthorizedException(ErrorMessages.MustBeLoggedIn);
}
