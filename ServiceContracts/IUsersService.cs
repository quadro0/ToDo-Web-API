using ServiceContracts.DTO;

namespace ServiceContracts
{
    public interface IUsersService
    {
        /// <summary>
        /// Creates new user, throws if fails
        /// </summary>
        /// <param name="userAddRequest">Model of the user that is being added</param>
        Task RegisterAsync(UserAddRequest userAddRequest);

        /// <summary>
        /// Authenticates user and generates JWT Token, throws if fails
        /// </summary>
        /// <param name="userLoginRequest">Model of the user that tries to login</param>
        /// <returns>Generated JWT Token</returns>
        Task<string> LoginAsync(UserLoginRequest userLoginRequest);

        /// <summary>
        /// Updates user's password, throws if fails
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="currentPassword"></param>
        /// <param name="newPassword"></param>
        /// <returns></returns>
        Task UpdatePasswordAsync(Guid userId, UserUpdateRequest userUpdateRequest);
    }
}
