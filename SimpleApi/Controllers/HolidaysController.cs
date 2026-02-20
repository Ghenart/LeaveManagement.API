using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SimpleApi.DTOs;
using SimpleApi.Models;
using SimpleApi.Service.Services;
using System;
using System.Threading.Tasks;

namespace SimpleApi.Controllers
{
    [Authorize(Roles = "Admin")]
    [ApiController]
    [Route("api/[controller]")]
    public class HolidaysController : ControllerBase
    {
        private readonly IHolidayService _holidayService;

        public HolidaysController(IHolidayService holidayService)
        {
            _holidayService = holidayService;
        }

        [HttpGet]
        public async Task<IActionResult> GetHolidays()
        {
            var holidays = await _holidayService.GetHolidaysAsync();
            return Ok(holidays);
        }

        [HttpPost]
        public async Task<IActionResult> CreateHoliday(HolidayCreateDto dto)
        {
            try
            {
                var holiday = await _holidayService.CreateHolidayAsync(dto);
                return Ok(new { Message = "Tatil başarıyla eklendi.", Holiday = holiday });
            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
        }


        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteHoliday(int id)
        {
            try
            {
                await _holidayService.DeleteHolidayAsync(id);
                return Ok(new { Message = "Tatil silindi." });
            }
            catch (Exception ex) when (ex.Message == "NOT_FOUND")
            {
                return NotFound();
            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
        }
        [AllowAnonymous]
        [HttpGet("test-error")]
        public IActionResult TestError()
        {
            // Sistem bu satırı okuduğunda bilerek hata fırlatacak
            throw new Exception("Loglama ve Middleware testi başarılı: Beklenmedik hata simülasyonu!");
        }

    }
}