namespace ServiceContracts
{
    public interface ITokensService
    {
        string GenerateToken(Guid userId);
    }
}
