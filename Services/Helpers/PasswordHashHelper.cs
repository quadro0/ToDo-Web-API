namespace Services.Helpers
{
    public static class PasswordHashHelper
    {
        public static string Generate(string password) 
        {
            return BCrypt.Net.BCrypt.EnhancedHashPassword(password);
        }

        public static bool Verify(string password, string storedHash)
        {
            return BCrypt.Net.BCrypt.EnhancedVerify(password, storedHash);
        }
    }
}
