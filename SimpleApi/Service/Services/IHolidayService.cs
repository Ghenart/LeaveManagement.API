using SimpleApi.DTOs;
using SimpleApi.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SimpleApi.Service.Services
{
    public interface IHolidayService
    {
        Task<IEnumerable<Holiday>> GetHolidaysAsync();
        Task<Holiday> CreateHolidayAsync(HolidayCreateDto dto);
        Task DeleteHolidayAsync(int id);
    }
}
