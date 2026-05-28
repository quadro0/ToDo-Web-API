namespace ServiceContracts
{
    public interface IPasswordService
    {
        public string Generate(string password);
        public bool Verify(string password, string storedHash);
    }
}
