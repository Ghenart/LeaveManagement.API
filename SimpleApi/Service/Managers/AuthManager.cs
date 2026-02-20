using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using SimpleApi.Core.Repositories;
using SimpleApi.Core.UnitOfWorks;
using SimpleApi.DTOs;
using SimpleApi.Models;
using SimpleApi.Service.Services;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace SimpleApi.Service.Managers
{
    public class AuthManager : IAuthService
    {
        private readonly IGenericRepository<User> _userRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IConfiguration _configuration;

        public AuthManager(IGenericRepository<User> userRepository, IUnitOfWork unitOfWork, IConfiguration configuration)
        {
            _userRepository = userRepository;
            _unitOfWork = unitOfWork;
            _configuration = configuration;
        }

        public async Task RegisterAsync(UserDto request)
        {
            var existingUsers = await _userRepository.FindAsync(u => u.Username == request.Username);
            if (existingUsers.Any())
                throw new Exception("BAD_REQUEST:Bu kullanıcı adı zaten kullanılıyor.");

            string passwordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);

            var user = new User
            {
                Username = request.Username,
                PasswordHash = passwordHash,
                Role = "Employee",
                LeaveBalance = 14
            };

            await _userRepository.AddAsync(user);
            await _unitOfWork.CommitAsync();
        }

        public async Task<string> LoginAsync(UserDto request)
        {
            var userList = await _userRepository.FindAsync(u => u.Username == request.Username);
            var user = userList.FirstOrDefault();

            if (user == null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
                throw new Exception("BAD_REQUEST:Kullanıcı adı veya şifre hatalı.");

            return CreateToken(user);
        }

        public async Task<object> GetProfileAsync(string? username)
        {
            var userList = await _userRepository.FindAsync(u => u.Username == username);
            var user = userList.FirstOrDefault();

            if (user == null)
                throw new Exception("NOT_FOUND");

            return new
            {
                user.Username,
                user.Role,
                user.LeaveBalance
            };
        }

        public async Task ChangePasswordAsync(ChangePasswordDto request, string? username)
        {
            var userList = await _userRepository.FindAsync(u => u.Username == username);
            var user = userList.FirstOrDefault();

            if (user == null)
                throw new Exception("NOT_FOUND");

            if (!BCrypt.Net.BCrypt.Verify(request.OldPassword, user.PasswordHash))
                throw new Exception("BAD_REQUEST:Eski şifreniz hatalı.");

            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);
            _userRepository.Update(user);
            await _unitOfWork.CommitAsync();
        }

        private string CreateToken(User user)
        {
            var jwtKey = _configuration["Jwt:Key"];
            var keyBytes = Encoding.UTF8.GetBytes(jwtKey!);

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.Role, user.Role)
            };

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddHours(2),
                Issuer = _configuration["Jwt:Issuer"],
                Audience = _configuration["Jwt:Audience"],
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(keyBytes), SecurityAlgorithms.HmacSha256Signature)
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);

            return tokenHandler.WriteToken(token);
        }
    }
}
