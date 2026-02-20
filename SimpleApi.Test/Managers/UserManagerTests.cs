using Bogus;
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
    public class UserManagerTests
    {
        private readonly Mock<IGenericRepository<User>> _mockUserRepo;
        private readonly Mock<IUnitOfWork> _mockUow;
        private readonly UserManager _userManager;

        public UserManagerTests()
        {
            _mockUserRepo = new Mock<IGenericRepository<User>>();
            _mockUow = new Mock<IUnitOfWork>();
            _userManager = new UserManager(_mockUserRepo.Object, _mockUow.Object);
        }

        [Fact]
        public async Task CreateUserAsync_KullaniciAdiKayitliysa_HataFirlatir()
        {
            // Arrange
            // Sektör standardı Bogus kütüphanesi ile sahte kullanıcı adı üretiyoruz
            var faker = new Faker();
            var fakeUsername = faker.Internet.UserName();

            var existingUsers = new List<User> { new User { Username = fakeUsername } };

            _mockUserRepo.Setup(x => x.FindAsync(It.IsAny<Expression<Func<User, bool>>>()))
                         .ReturnsAsync(existingUsers);

            var dto = new UserCreateDto { Username = fakeUsername, Password = "123", Role = "Employee", LeaveBalance = 14 };

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(() => _userManager.CreateUserAsync(dto));
            Assert.Contains("Bu kullanıcı adı zaten sistemde kayıtlı", exception.Message);
        }

        [Fact]
        public async Task DeactivateUserAsync_KendiHesabiniKapatmayaCalisirsa_HataFirlatir()
        {
            // Arrange
            var currentUsername = "admin_user";
            var user = new User { Id = 1, Username = currentUsername, IsActive = true };

            _mockUserRepo.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(user);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(() => _userManager.DeactivateUserAsync(1, currentUsername));
            Assert.Contains("Kendi hesabınızı pasife alamazsınız", exception.Message);
        }

        [Fact]
        public async Task DeactivateUserAsync_AktifKullaniciYoksa_HataFirlatir()
        {
            // Arrange
            User nullUser = null;
            _mockUserRepo.Setup(x => x.GetByIdAsync(99)).ReturnsAsync(nullUser);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(() => _userManager.DeactivateUserAsync(99, "admin_user"));
            Assert.Contains("Aktif bir kullanıcı bulunamadı", exception.Message);
        }
    }
}