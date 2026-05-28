namespace ServiceContracts
{
    public interface ITokensService
    {
        /// <summary>
        /// Generates JWT token
        /// </summary>
        /// <param name="userId">UserId to be included in token</param>
        /// <returns>Returns generated JWT token as a string</returns>
        string GenerateToken(Guid userId);
    }
}
