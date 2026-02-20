using SimpleApi.DTOs;
using System.Threading.Tasks;

namespace SimpleApi.Service.Services
{
    public interface IAuthService
    {
        Task RegisterAsync(UserDto request);
        Task<string> LoginAsync(UserDto request);
        Task<object> GetProfileAsync(string? username);
        Task ChangePasswordAsync(ChangePasswordDto request, string? username);
    }
}
