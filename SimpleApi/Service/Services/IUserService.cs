using SimpleApi.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SimpleApi.Service.Services
{
    public interface IUserService
    {
        Task<IEnumerable<object>> GetAllUsersAsync();
        Task<int> CreateUserAsync(UserCreateDto dto);
        Task DeactivateUserAsync(int id, string? currentUsername);
    }
}
