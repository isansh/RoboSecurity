using BCrypt.Net;

namespace RoboSecurity.BLL.Helpers
{
    public static class HashCodeHelper
    {
        public static string HashPassword(string password)
        {
            if (!VerifyingRequestHelper.VerifyQuery(password))
            {
                return string.Empty;
            }

            return BCrypt.Net.BCrypt.HashPassword(password);
        }

        public static bool VerifyPassword(string password, string hashedPassword)
        {
            if (!VerifyingRequestHelper.VerifyQuery(password) ||
                !VerifyingRequestHelper.VerifyQuery(hashedPassword))
            {
                return false;
            }

            return BCrypt.Net.BCrypt.Verify(password, hashedPassword);
        }
    }
}
