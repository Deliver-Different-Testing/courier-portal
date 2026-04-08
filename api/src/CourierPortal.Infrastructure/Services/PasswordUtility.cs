using System.Security.Cryptography;
using CourierPortal.Infrastructure.Models;

namespace CourierPortal.Infrastructure.Services
{
    /// <summary>
    /// Utility to help with passwords, for consistency, hashing logic is taken from existing UrgentPublicMVC project
    /// </summary>
    public class PasswordUtility
    {        
        /// <summary>
        /// call to create or check/verify a hashed password using the unique salt value
        /// </summary>
        /// <param name="password"></param>
        /// <param name="salt"></param>
        /// <returns></returns>
        public static string HashPassword(string password, string salt)
        {
            var k2 = new Rfc2898DeriveBytes(password, System.Text.Encoding.UTF8.GetBytes(salt + salt));
            var result = System.Text.Encoding.UTF8.GetString(k2.GetBytes(128));
            return result;
        }

        /// <summary>
        /// Call to correctly set the unique salt value and hash the new password for a user
        /// </summary>
        /// <param name="password"></param>
        /// <returns></returns>
        public static SaltHashed SaltHashNewPassword(string password)
        {
            var salt = RandomNumber.Next(100000, 999999).ToString();
            var result = new SaltHashed
            {
                Salt = salt,
                Hashed = HashPassword(password, salt)
            };
            return result;
        }
    }
}
