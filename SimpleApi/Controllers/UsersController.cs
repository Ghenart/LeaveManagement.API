using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SimpleApi.DTOs;
using SimpleApi.Service.Services;
using System;
using System.Threading.Tasks;

namespace SimpleApi.Controllers
{
    [Authorize(Roles = "Admin")]
    [ApiController]
    [Route("api/[controller]")]
    public class UsersController : ControllerBase
    {
        private readonly IUserService _userService;

        public UsersController(IUserService userService)
        {
            _userService = userService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllUsers()
        {
            var users = await _userService.GetAllUsersAsync();
            return Ok(users);
        }

        [HttpPost]
        public async Task<IActionResult> CreateUser(UserCreateDto dto)
        {
            try
            {
                var userId = await _userService.CreateUserAsync(dto);
                return Ok(new { Message = "Yeni personel başarıyla eklendi.", UserId = userId });
            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeactivateUser(int id)
        {
            try
            {
                await _userService.DeactivateUserAsync(id, User.Identity?.Name);
                return Ok(new { Message = "Personel hesabı pasife alındı ve sistem erişimi kesildi." });
            }
            catch (Exception ex) when (ex.Message.StartsWith("NOT_FOUND:"))
            {
                return NotFound(new { Message = ex.Message.Substring("NOT_FOUND:".Length) });
            }
            catch (Exception ex) when (ex.Message.StartsWith("BAD_REQUEST:"))
            {
                return BadRequest(new { Message = ex.Message.Substring("BAD_REQUEST:".Length) });
            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
        }
    }
}