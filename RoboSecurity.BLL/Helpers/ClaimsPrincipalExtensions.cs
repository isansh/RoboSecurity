using System.Security.Claims;

namespace RoboSecurity.BLL.Helpers
{
    public static class ClaimsPrincipalExtensions
    {
        public static int GetUserId(this ClaimsPrincipal user)
        {
            var userIdClaim = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim))
            {
                userIdClaim = user.FindFirst("userId")?.Value;
            }
            return int.Parse(userIdClaim ?? "0");
        }
    }
}