using Microsoft.IdentityModel.Tokens;
using RoboSecurity.DAL.Models;

namespace RoboSecurity.BLL.Helpers
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