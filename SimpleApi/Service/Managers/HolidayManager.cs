using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SimpleApi.Core.Repositories;
using SimpleApi.Core.UnitOfWorks;
using SimpleApi.DTOs;
using SimpleApi.Models;
using SimpleApi.Service.Services;

namespace SimpleApi.Service.Managers
{
    public class HolidayManager : IHolidayService
    {
        private readonly IGenericRepository<Holiday> _holidayRepository;
        private readonly IUnitOfWork _unitOfWork;

        public HolidayManager(IGenericRepository<Holiday> holidayRepository, IUnitOfWork unitOfWork)
        {
            _holidayRepository = holidayRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<IEnumerable<Holiday>> GetHolidaysAsync()
        {
            var holidays = await _holidayRepository.GetAllAsync();
            return holidays.OrderBy(h => h.Date);
        }

        public async Task<Holiday> CreateHolidayAsync(HolidayCreateDto dto)
        {
            var existingHolidays = await _holidayRepository.FindAsync(h => h.Date.Date == dto.Date.Date);
            if (existingHolidays.Any())
                throw new Exception("Bu tarihte zaten bir tatil tanımlanmış.");

            var holiday = new Holiday
            {
                Name = dto.Name,
                Date = dto.Date.Date
            };

            await _holidayRepository.AddAsync(holiday);
            await _unitOfWork.CommitAsync();

            return holiday;
        }

        public async Task DeleteHolidayAsync(int id)
        {
            var holiday = await _holidayRepository.GetByIdAsync(id);
            if (holiday == null)
                throw new Exception("NOT_FOUND");

            _holidayRepository.Remove(holiday);
            await _unitOfWork.CommitAsync();
        }
    }
}
