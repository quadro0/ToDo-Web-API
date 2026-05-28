using ServiceContracts.DTO;

namespace ServiceContracts
{
    public interface IUsersService
    {
        Task RegisterAsync(UserAddRequest userAddRequest);
        Task<string> LoginAsync(UserLoginRequest userLoginRequest);
        Task UpdatePasswordAsync(Guid userId, UserUpdateRequest userUpdateRequest);
    }
}
