using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SimpleApi.DTOs;
using SimpleApi.Service.Services;
using System;
using System.Threading.Tasks;

namespace SimpleApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register(UserDto request)
        {
            try
            {
                await _authService.RegisterAsync(request);
                return Ok(new { Message = "Kullanıcı başarıyla oluşturuldu." });
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

        [HttpPost("login")]
        public async Task<IActionResult> Login(UserDto request)
        {
            try
            {
                var token = await _authService.LoginAsync(request);
                return Ok(new { Token = token });
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

        [Authorize]
        [HttpGet("profile")]
        public async Task<IActionResult> GetProfile()
        {
            try
            {
                var user = await _authService.GetProfileAsync(User.Identity?.Name);
                return Ok(user);
            }
            catch (Exception ex) when (ex.Message == "NOT_FOUND")
            {
                return NotFound();
            }
        }

        [Authorize]
        [HttpPatch("change-password")]
        public async Task<IActionResult> ChangePassword(ChangePasswordDto request)
        {
            try
            {
                await _authService.ChangePasswordAsync(request, User.Identity?.Name);
                return Ok(new { Message = "Şifreniz başarıyla güncellendi." });
            }
            catch (Exception ex) when (ex.Message == "NOT_FOUND")
            {
                return NotFound();
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