using Api.Swazy.Common;
using BCryptNet = BCrypt.Net.BCrypt;

namespace Api.Swazy.Providers
{
    public class HashingProvider : IHashingProvider
    {
        private static string GetRandomSalt()
        {
            return BCryptNet.GenerateSalt(SwazyConstants.HashSaltWorkforce);
        }

        public string HashPassword(string password)
        {
            return BCryptNet.HashPassword(password, GetRandomSalt());
        }

        public bool ValidatePassword(string password, string correctHash)
        {
            return BCryptNet.Verify(password, correctHash);
        }
    }
}
