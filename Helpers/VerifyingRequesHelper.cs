using Microsoft.IdentityModel.Tokens;
using RoboSecurity.Models;

namespace RoboSecurity.Helpers
{
    public static class VerifyingRequestHelper
    {
        public static bool VerifyQuery(string str)
        {
            if (string.IsNullOrWhiteSpace(str) || string.IsNullOrEmpty(str))
            {
                return false;
            }

            return true;
        }
    }
}