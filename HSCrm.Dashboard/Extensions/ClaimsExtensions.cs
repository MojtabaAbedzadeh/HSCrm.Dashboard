using System.Security.Claims;

namespace HSCrm.Dashboard.Extensions
{
    public static class ClaimsExtensions
    {
        public static bool HasPermission(this ClaimsPrincipal user, string permission)
        {
            return user.HasClaim("permission", permission);
        }
    }
}