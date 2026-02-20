using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SimpleApi.Core.Repositories;
using SimpleApi.Core.UnitOfWorks;
using SimpleApi.DTOs;
using SimpleApi.Models;
using SimpleApi.Service.Services;

namespace SimpleApi.Service.Managers
{
    public class UserManager : IUserService
    {
        private readonly IGenericRepository<User> _userRepository;
        private readonly IUnitOfWork _unitOfWork;

        public UserManager(IGenericRepository<User> userRepository, IUnitOfWork unitOfWork)
        {
            _userRepository = userRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<IEnumerable<object>> GetAllUsersAsync()
        {
            var users = await _userRepository.FindAsync(u => u.IsActive);
            return users.Select(u => new
            {
                u.Id,
                u.Username,
                u.Role,
                u.LeaveBalance
            }).ToList();
        }

        public async Task<int> CreateUserAsync(UserCreateDto dto)
        {
            var existingUsers = await _userRepository.FindAsync(u => u.Username == dto.Username);
            if (existingUsers.Any())
                throw new Exception("Bu kullanıcı adı zaten sistemde kayıtlı.");

            var newUser = new User
            {
                Username = dto.Username,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
                Role = dto.Role,
                LeaveBalance = dto.LeaveBalance,
                IsActive = true
            };

            await _userRepository.AddAsync(newUser);
            await _unitOfWork.CommitAsync();

            return newUser.Id;
        }

        public async Task DeactivateUserAsync(int id, string? currentUsername)
        {
            var user = await _userRepository.GetByIdAsync(id);

            if (user == null || !user.IsActive)
                throw new Exception("NOT_FOUND:Aktif bir kullanıcı bulunamadı.");

            if (user.Username == currentUsername)
                throw new Exception("BAD_REQUEST:Kendi hesabınızı pasife alamazsınız.");

            user.IsActive = false;
            _userRepository.Update(user);
            await _unitOfWork.CommitAsync();
        }
    }
}
