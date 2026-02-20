using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SimpleApi.DTOs;
using SimpleApi.Models;
using SimpleApi.Service.Services;
using System;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace SimpleApi.Controllers.Controller
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class LeavesController : ControllerBase
    {
        private readonly ILeaveService _leaveService;

        public LeavesController(ILeaveService leaveService)
        {
            _leaveService = leaveService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] string? status, [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
        {
            var currentUsername = User.Identity?.Name;
            var role = User.FindFirstValue(ClaimTypes.Role);

            var info = await _leaveService.GetPaginationInfoAsync(currentUsername, role, status, pageSize);
            var items = await _leaveService.GetAllLeavesAsync(currentUsername, role, status, pageNumber, pageSize);

            var data = items.Select(x => new
            {
                x.Id,
                Username = x.User!.Username,
                x.StartDate,
                x.EndDate,
                x.Status,
                x.DocumentPath,
                x.LeaveType
            }).ToList();

            return Ok(new
            {
                TotalRecords = info.TotalRecords,
                TotalPages = info.TotalPages,
                CurrentPage = pageNumber,
                PageSize = pageSize,
                Data = data
            });
        }

        [HttpPost]
        public async Task<IActionResult> Create(LeaveRequestCreateDto dto)
        {
            if (dto.EndDate < dto.StartDate) return BadRequest(new { Message = "Bitiş tarihi başlangıç tarihinden önce olamaz." });

            var leaveRequest = new LeaveRequest
            {
                StartDate = dto.StartDate,
                EndDate = dto.EndDate,
                LeaveType = dto.LeaveType
            };

            try
            {
                var createdLeave = await _leaveService.CreateLeaveAsync(leaveRequest, User.Identity?.Name);
                return Ok(new { Message = "İzin talebi başarıyla oluşturuldu.", LeaveId = createdLeave.Id });
            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                await _leaveService.DeleteLeaveAsync(id, User.Identity?.Name, User.FindFirstValue(ClaimTypes.Role));
                return NoContent();
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
        }

        [Authorize(Roles = "Admin")]
        [HttpGet("dashboard")]
        public async Task<IActionResult> GetDashboard()
        {
            var dashboardData = await _leaveService.GetDashboardAsync();
            return Ok(dashboardData);
        }

        [HttpPost("{id}/upload-document")]
        public async Task<IActionResult> UploadDocument(int id, IFormFile file)
        {
            if (file == null || file.Length == 0) return BadRequest("Geçerli bir dosya yükleyin.");

            var folderPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");

            try
            {
                var fileName = await _leaveService.UploadDocumentAsync(id, file, User.Identity?.Name, User.FindFirstValue(ClaimTypes.Role), folderPath);
                return Ok(new { Message = "Dosya yüklendi.", File = fileName });
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
        }

        [HttpPatch("{id}/cancel")]
        public async Task<IActionResult> CancelLeave(int id)
        {
            try
            {
                var newBalance = await _leaveService.CancelLeaveAsync(id, User.Identity?.Name, User.FindFirstValue(ClaimTypes.Role));
                return Ok(new { Message = "İzin başarıyla iptal edildi ve bakiye iade edildi.", NewBalance = newBalance });
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
        }

        [Authorize(Roles = "Admin")]
        [HttpPatch("{id}/status")]
        public async Task<IActionResult> UpdateStatus(int id, [FromBody] string newStatus)
        {
            try
            {
                var currentBalance = await _leaveService.UpdateStatusAsync(id, newStatus);
                return Ok(new { Message = $"İzin durumu {newStatus} olarak güncellendi.", CurrentBalance = currentBalance });
            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
        }
    }
}