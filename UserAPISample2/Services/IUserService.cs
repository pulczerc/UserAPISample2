using UserAPISample2.Models;

namespace UserAPISample2.Services
{
    public interface IUserService
    {
        Task<User?> GetUserAsync(string id);
        Task<IEnumerable<User>> GetUsersAsync();
        Task<User> CreateUserAsync(User user);
        Task<bool> UpdateUserAsync(string id, User user);
        Task<bool> DeleteUserAsync(string id);
    }
}
