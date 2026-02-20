using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using SimpleApi.Core.Repositories;
using SimpleApi.Core.UnitOfWorks;
using SimpleApi.Models;
using SimpleApi.Service.Services;

namespace SimpleApi.Service.Managers
{
    public class LeaveManager : ILeaveService
    {
        private readonly IGenericRepository<LeaveRequest> _leaveRepository;
        private readonly IGenericRepository<User> _userRepository;
        private readonly IGenericRepository<Holiday> _holidayRepository;
        private readonly IGenericRepository<AuditLog> _auditLogRepository;
        private readonly IUnitOfWork _unitOfWork;

        // Include işlemleri için geçici olarak Context kullanılıyor. 
        // İleride Specification Pattern eklendiğinde bu tamamen silinebilir.
        private readonly ApiDbContext _context;

        public LeaveManager(
            IGenericRepository<LeaveRequest> leaveRepository,
            IGenericRepository<User> userRepository,
            IGenericRepository<Holiday> holidayRepository,
            IGenericRepository<AuditLog> auditLogRepository,
            IUnitOfWork unitOfWork,
            ApiDbContext context)
        {
            _leaveRepository = leaveRepository;
            _userRepository = userRepository;
            _holidayRepository = holidayRepository;
            _auditLogRepository = auditLogRepository;
            _unitOfWork = unitOfWork;
            _context = context;
        }

        public async Task<(int TotalRecords, int TotalPages)> GetPaginationInfoAsync(string? currentUsername, string? role, string? status, int pageSize)
        {
            var query = _context.LeaveRequests.Where(x => !x.IsDeleted).AsQueryable();

            if (role != "Admin") query = query.Where(x => x.User!.Username == currentUsername);
            if (!string.IsNullOrEmpty(status)) query = query.Where(x => x.Status == status);

            var totalRecords = await query.CountAsync();
            var totalPages = (int)Math.Ceiling(totalRecords / (double)pageSize);

            return (totalRecords, totalPages);
        }

        public async Task<IEnumerable<LeaveRequest>> GetAllLeavesAsync(string? currentUsername, string? role, string? status, int pageNumber, int pageSize)
        {
            var query = _context.LeaveRequests.Include(x => x.User).Where(x => !x.IsDeleted).AsQueryable();

            if (role != "Admin") query = query.Where(x => x.User!.Username == currentUsername);
            if (!string.IsNullOrEmpty(status)) query = query.Where(x => x.Status == status);

            return await query.OrderByDescending(x => x.Id).Skip((pageNumber - 1) * pageSize).Take(pageSize).ToListAsync();
        }

        public async Task<LeaveRequest> CreateLeaveAsync(LeaveRequest leaveRequest, string? username)
        {
            var currentUser = await _context.Users.FirstOrDefaultAsync(u => u.Username == username);
            if (currentUser == null) throw new Exception("Kullanıcı bulunamadı.");

            var requestedDays = await CalculateBusinessDays(leaveRequest.StartDate, leaveRequest.EndDate);

            if (leaveRequest.LeaveType != "Raporlu" && leaveRequest.LeaveType != "Ücretsiz İzin" && requestedDays > currentUser.LeaveBalance)
                throw new Exception($"Yetersiz izin bakiyesi. Kalan hakkınız: {currentUser.LeaveBalance} gün.");

            leaveRequest.UserId = currentUser.Id;
            leaveRequest.Status = "Pending";

            await _leaveRepository.AddAsync(leaveRequest);
            await _unitOfWork.CommitAsync();
            await LogAction("İzin Talebi Oluşturuldu", "LeaveRequest", leaveRequest.Id, username);

            return leaveRequest;
        }

        public async Task DeleteLeaveAsync(int id, string? currentUsername, string? role)
        {
            var leave = await _context.LeaveRequests.Include(x => x.User).FirstOrDefaultAsync(x => x.Id == id);
            if (leave == null || leave.IsDeleted) throw new Exception("İzin kaydı bulunamadı.");
            if (role != "Admin" && leave.User!.Username != currentUsername) throw new UnauthorizedAccessException();
            if (leave.Status != "Pending" && role != "Admin") throw new Exception("Sadece beklemede olan izinler silinebilir.");

            leave.IsDeleted = true;
            _leaveRepository.Update(leave);
            await _unitOfWork.CommitAsync();
            await LogAction("İzin Silindi", "LeaveRequest", leave.Id, currentUsername);
        }

        public async Task<object> GetDashboardAsync()
        {
            var today = DateTime.Today;
            var query = _context.LeaveRequests.Include(x => x.User).Where(x => !x.IsDeleted);

            var totalStats = new
            {
                TotalRequests = await query.CountAsync(),
                ApprovedCount = await query.CountAsync(x => x.Status == "Approved"),
                PendingCount = await query.CountAsync(x => x.Status == "Pending"),
                RejectedCount = await query.CountAsync(x => x.Status == "Rejected")
            };

            var pendingRequests = await query.Where(x => x.Status == "Pending").OrderBy(x => x.StartDate)
                .Select(x => new { x.Id, Username = x.User!.Username, x.StartDate, x.EndDate, x.LeaveType }).ToListAsync();

            var currentlyOnLeave = await query.Where(x => x.Status == "Approved" && today >= x.StartDate && today <= x.EndDate)
                .Select(x => new { x.Id, x.User!.Username, x.StartDate, x.EndDate, x.LeaveType }).ToListAsync();

            var upcomingLeaves = await query.Where(x => x.Status == "Approved" && x.StartDate > today).OrderBy(x => x.StartDate)
                .Take(5).Select(x => new { x.Id, x.User!.Username, x.StartDate, x.EndDate }).ToListAsync();

            return new { Stats = totalStats, PendingRequests = pendingRequests, CurrentlyOnLeave = currentlyOnLeave, UpcomingLeaves = upcomingLeaves };
        }

        public async Task<string> UploadDocumentAsync(int id, IFormFile file, string? currentUsername, string? role, string folderPath)
        {
            var leave = await _context.LeaveRequests.Include(x => x.User).FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted);
            if (leave == null) throw new Exception("İzin kaydı bulunamadı.");
            if (role != "Admin" && leave.User!.Username != currentUsername) throw new UnauthorizedAccessException();

            if (!Directory.Exists(folderPath)) Directory.CreateDirectory(folderPath);

            var fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
            var filePath = Path.Combine(folderPath, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            leave.DocumentPath = fileName;
            _leaveRepository.Update(leave);
            await _unitOfWork.CommitAsync();

            return fileName;
        }

        public async Task<int> CancelLeaveAsync(int id, string? currentUsername, string? role)
        {
            var leave = await _context.LeaveRequests.Include(x => x.User).FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted);
            if (leave == null) throw new Exception("İzin kaydı bulunamadı.");
            if (role != "Admin" && leave.User!.Username != currentUsername) throw new UnauthorizedAccessException();
            if (leave.Status == "Cancelled" || leave.Status == "Rejected") throw new Exception("Bu izin zaten iptal edilmiş veya reddedilmiş.");

            if (leave.Status == "Approved" && leave.LeaveType != "Raporlu" && leave.LeaveType != "Ücretsiz İzin")
            {
                int daysToReturn = await CalculateBusinessDays(leave.StartDate, leave.EndDate);
                leave.User!.LeaveBalance += daysToReturn;
            }

            leave.Status = "Cancelled";
            _leaveRepository.Update(leave);
            await _unitOfWork.CommitAsync();
            await LogAction("İzin İptal Edildi", "LeaveRequest", leave.Id, currentUsername);

            return leave.User!.LeaveBalance;
        }

        public async Task<int> UpdateStatusAsync(int id, string newStatus)
        {
            var leave = await _context.LeaveRequests.Include(x => x.User).FirstOrDefaultAsync(x => x.Id == id);
            if (leave == null || leave.IsDeleted) throw new Exception("İzin kaydı bulunamadı.");

            var validStatuses = new[] { "Approved", "Rejected", "Pending" };
            if (!validStatuses.Contains(newStatus)) throw new Exception("Geçersiz durum bilgisi.");

            if (newStatus == "Approved" && leave.Status != "Approved")
            {
                if (leave.LeaveType != "Raporlu" && leave.LeaveType != "Ücretsiz İzin")
                {
                    int requestedDays = await CalculateBusinessDays(leave.StartDate, leave.EndDate);
                    if (leave.User!.LeaveBalance < requestedDays) throw new Exception("Kullanıcının bakiyesi yetersiz.");
                    leave.User.LeaveBalance -= requestedDays;
                }
            }
            else if (leave.Status == "Approved" && newStatus != "Approved")
            {
                if (leave.LeaveType != "Raporlu" && leave.LeaveType != "Ücretsiz İzin")
                {
                    int requestedDays = await CalculateBusinessDays(leave.StartDate, leave.EndDate);
                    leave.User!.LeaveBalance += requestedDays;
                }
            }

            leave.Status = newStatus;
            _leaveRepository.Update(leave);
            await _unitOfWork.CommitAsync();
            await LogAction($"İzin Durumu Değiştirildi: {newStatus}", "LeaveRequest", leave.Id, null);

            return leave.User!.LeaveBalance;
        }

        private async Task<int> CalculateBusinessDays(DateTime startDate, DateTime endDate)
        {
            int businessDays = 0;
            DateTime currentDate = startDate;
            var holidays = await _holidayRepository.FindAsync(h => h.Date >= startDate && h.Date <= endDate);
            var holidayDates = holidays.Select(h => h.Date.Date).ToList();

            while (currentDate <= endDate)
            {
                bool isWeekend = currentDate.DayOfWeek == DayOfWeek.Saturday || currentDate.DayOfWeek == DayOfWeek.Sunday;
                if (!isWeekend && !holidayDates.Contains(currentDate.Date)) businessDays++;
                currentDate = currentDate.AddDays(1);
            }
            return businessDays;
        }

        private async Task LogAction(string action, string entityName, int entityId, string? username)
        {
            var log = new AuditLog
            {
                Username = username ?? "Sistem",
                Action = action,
                EntityName = entityName,
                EntityId = entityId,
                Timestamp = DateTime.Now
            };
            await _auditLogRepository.AddAsync(log);
        }
    }
}