using Microsoft.AspNetCore.Http;
using SimpleApi.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SimpleApi.Service.Services
{
    public interface ILeaveService
    {
        Task<IEnumerable<LeaveRequest>> GetAllLeavesAsync(string? currentUsername, string? role, string? status, int pageNumber, int pageSize);
        Task<(int TotalRecords, int TotalPages)> GetPaginationInfoAsync(string? currentUsername, string? role, string? status, int pageSize);
        Task<LeaveRequest> CreateLeaveAsync(LeaveRequest leaveRequest, string? username);
        Task DeleteLeaveAsync(int id, string? currentUsername, string? role);
        Task<object> GetDashboardAsync();
        Task<string> UploadDocumentAsync(int id, IFormFile file, string? currentUsername, string? role, string folderPath);
        Task<int> CancelLeaveAsync(int id, string? currentUsername, string? role);
        Task<int> UpdateStatusAsync(int id, string newStatus);
    }
}