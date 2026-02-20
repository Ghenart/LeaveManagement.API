using Microsoft.Extensions.Configuration;
using Moq;
using SimpleApi.Core.Repositories;
using SimpleApi.Core.UnitOfWorks;
using SimpleApi.DTOs;
using SimpleApi.Models;
using SimpleApi.Service.Managers;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Xunit;

namespace SimpleApi.Test.Managers
{
    public class AuthManagerTests
    {
        private readonly Mock<IGenericRepository<User>> _mockUserRepo;
        private readonly Mock<IUnitOfWork> _mockUow;
        private readonly IConfiguration _configuration;
        private readonly AuthManager _authManager;

        public AuthManagerTests()
        {
            _mockUserRepo = new Mock<IGenericRepository<User>>();
            _mockUow = new Mock<IUnitOfWork>();

            // Sektör Standardı: Konfigürasyon dosyasını (appsettings.json) hafızada taklit etmek
            var inMemorySettings = new Dictionary<string, string> {
                {"Jwt:Key", "bu_test_icin_olusturulmus_gizli_anahtar_12345"},
                {"Jwt:Issuer", "TestIssuer"},
                {"Jwt:Audience", "TestAudience"}
            };

            _configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(inMemorySettings)
                .Build();

            _authManager = new AuthManager(_mockUserRepo.Object, _mockUow.Object, _configuration);
        }

        [Fact]
        public async Task RegisterAsync_KullaniciAdiKayitliysa_HataFirlatir()
        {
            // Arrange
            var dto = new UserDto { Username = "testuser", Password = "123" };
            var existingUsers = new List<User> { new User { Username = "testuser" } };

            _mockUserRepo.Setup(x => x.FindAsync(It.IsAny<Expression<Func<User, bool>>>()))
                         .ReturnsAsync(existingUsers);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(() => _authManager.RegisterAsync(dto));
            Assert.Contains("Bu kullanıcı adı zaten kullanılıyor", exception.Message);
        }

        [Fact]
        public async Task LoginAsync_SifreHataliysa_HataFirlatir()
        {
            // Arrange
            var dto = new UserDto { Username = "testuser", Password = "yanlis_sifre" };
            var gercekSifreHash = BCrypt.Net.BCrypt.HashPassword("dogru_sifre");

            var existingUsers = new List<User> { new User { Username = "testuser", PasswordHash = gercekSifreHash } };

            _mockUserRepo.Setup(x => x.FindAsync(It.IsAny<Expression<Func<User, bool>>>()))
                         .ReturnsAsync(existingUsers);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(() => _authManager.LoginAsync(dto));
            Assert.Contains("Kullanıcı adı veya şifre hatalı", exception.Message);
        }

        [Fact]
        public async Task ChangePasswordAsync_EskiSifreHataliysa_HataFirlatir()
        {
            // Arrange
            var username = "testuser";
            var dto = new ChangePasswordDto { OldPassword = "yanlis_eski_sifre", NewPassword = "yeni_sifre" };
            var gercekSifreHash = BCrypt.Net.BCrypt.HashPassword("dogru_eski_sifre");

            var existingUsers = new List<User> { new User { Username = username, PasswordHash = gercekSifreHash } };

            _mockUserRepo.Setup(x => x.FindAsync(It.IsAny<Expression<Func<User, bool>>>()))
                         .ReturnsAsync(existingUsers);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(() => _authManager.ChangePasswordAsync(dto, username));
            Assert.Contains("Eski şifreniz hatalı", exception.Message);
        }
    }
}