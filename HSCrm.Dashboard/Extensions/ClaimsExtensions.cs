using System.Security.Claims;

namespace HSCrm.Dashboard.Extensions
{
    public static class ClaimsExtensions
    {
        public static bool HasPermission(this ClaimsPrincipal user, string permission)
        {
            return user.Claims.Any(c =>
                string.Equals(c.Type, "Permission", StringComparison.OrdinalIgnoreCase) &&
                string.Equals(c.Value, permission, StringComparison.OrdinalIgnoreCase));
        }
    }
}
