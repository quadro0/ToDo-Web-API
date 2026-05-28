using ServiceContracts;

namespace Services
{
    public class PasswordService : IPasswordService
    {
        public string Generate(string password)
        {
            return BCrypt.Net.BCrypt.EnhancedHashPassword(password);
        }

        public bool Verify(string password, string storedHash)
        {
            return BCrypt.Net.BCrypt.EnhancedVerify(password, storedHash);
        }
    }
}
