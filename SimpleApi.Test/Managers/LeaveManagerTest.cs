using Microsoft.EntityFrameworkCore;
using Moq;
using SimpleApi.Core.Repositories;
using SimpleApi.Core.UnitOfWorks;
using SimpleApi.Models;
using SimpleApi.Service.Managers;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Xunit;

namespace SimpleApi.Test.Managers
{
    public class LeaveManagerTests
    {
        // Kod tekrarını önlemek için her testte bize temiz bir sistem verecek yardımcı metot
        private (LeaveManager Manager, ApiDbContext Context, Mock<IGenericRepository<LeaveRequest>> LeaveRepo, Mock<IGenericRepository<Holiday>> HolidayRepo) GetManager()
        {
            var options = new DbContextOptionsBuilder<ApiDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            var context = new ApiDbContext(options);

            var mockLeaveRepo = new Mock<IGenericRepository<LeaveRequest>>();
            var mockUserRepo = new Mock<IGenericRepository<User>>();
            var mockHolidayRepo = new Mock<IGenericRepository<Holiday>>();
            var mockAuditRepo = new Mock<IGenericRepository<AuditLog>>();
            var mockUow = new Mock<IUnitOfWork>();

            var manager = new LeaveManager(
                mockLeaveRepo.Object, mockUserRepo.Object, mockHolidayRepo.Object,
                mockAuditRepo.Object, mockUow.Object, context);

            return (manager, context, mockLeaveRepo, mockHolidayRepo);
        }

        [Fact]
        public async Task CreateLeaveAsync_YetersizBakiye_HataFirlatmali()
        {
            var setup = GetManager();
            var testUser = new User { Id = 1, Username = "testuser", LeaveBalance = 5 };
            setup.Context.Users.Add(testUser);
            await setup.Context.SaveChangesAsync();

            var yeniIzin = new LeaveRequest
            {
                StartDate = DateTime.Now,
                EndDate = DateTime.Now.AddDays(10), // 5 gün hakkı var, 10 gün istiyor
                LeaveType = "Yıllık İzin"
            };

            var exception = await Assert.ThrowsAsync<Exception>(() =>
                setup.Manager.CreateLeaveAsync(yeniIzin, "testuser"));

            Assert.Contains("Yetersiz izin bakiyesi", exception.Message);
        }

        [Fact]
        public async Task CreateLeaveAsync_BakiyeYeterli_BasariylaOlusturur()
        {
            var setup = GetManager();
            var user = new User { Id = 1, Username = "johndoe", LeaveBalance = 14 };
            setup.Context.Users.Add(user);
            await setup.Context.SaveChangesAsync();

            // Sektör standardı: Sahte tatil veritabanının boş liste dönmesini sağlıyoruz
            setup.HolidayRepo.Setup(x => x.FindAsync(It.IsAny<Expression<Func<Holiday, bool>>>()))
                             .ReturnsAsync(new List<Holiday>());

            var leave = new LeaveRequest { StartDate = DateTime.Now, EndDate = DateTime.Now.AddDays(2), LeaveType = "Yıllık İzin" };

            var result = await setup.Manager.CreateLeaveAsync(leave, "johndoe");

            Assert.Equal("Pending", result.Status);
            // Repository'nin AddAsync metodunun tam olarak 1 kere çağrıldığını doğruluyoruz
            setup.LeaveRepo.Verify(x => x.AddAsync(It.IsAny<LeaveRequest>()), Times.Once);
        }

        [Fact]
        public async Task DeleteLeaveAsync_BaskaKullanici_YetkiHatasiVerir()
        {
            var setup = GetManager();
            var yetkisizKullanici = new User { Id = 1, Username = "saldirgan" };
            var izinSahibi = new User { Id = 2, Username = "gercek_sahip" };
            var leave = new LeaveRequest { Id = 1, UserId = 2, User = izinSahibi, Status = "Pending" };

            setup.Context.Users.AddRange(yetkisizKullanici, izinSahibi);
            setup.Context.LeaveRequests.Add(leave);
            await setup.Context.SaveChangesAsync();

            await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
                setup.Manager.DeleteLeaveAsync(1, "saldirgan", "User"));
        }

        [Fact]
        public async Task CancelLeaveAsync_IzinOnayliysa_BakiyeyiGeriYukler()
        {
            var setup = GetManager();
            var user = new User { Id = 1, Username = "johndoe", LeaveBalance = 10 };

            // Onaylanmış ve kullanıcıdan bakiyesi düşülmüş bir izin
            var leave = new LeaveRequest
            {
                Id = 1,
                UserId = 1,
                User = user,
                Status = "Approved",
                LeaveType = "Yıllık İzin",
                StartDate = DateTime.Now,
                EndDate = DateTime.Now.AddDays(2)
            };

            setup.Context.Users.Add(user);
            setup.Context.LeaveRequests.Add(leave);
            await setup.Context.SaveChangesAsync();

            setup.HolidayRepo.Setup(x => x.FindAsync(It.IsAny<Expression<Func<Holiday, bool>>>()))
                             .ReturnsAsync(new List<Holiday>());

            var yeniBakiye = await setup.Manager.CancelLeaveAsync(1, "johndoe", "User");

            Assert.Equal("Cancelled", leave.Status);
            Assert.True(yeniBakiye > 10); // İptal edildiği için kullanıcının 10 olan bakiyesi artmış olmalı
        }
    }
}