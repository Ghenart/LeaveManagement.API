using AutoMapper;
using SimpleApi.DTOs;
using SimpleApi.Models;

namespace SimpleApi.Mappings
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            // CreateMap<NeredenGelecek, NereyeGidecek>();

            CreateMap<UserCreateDto, User>();
            CreateMap<LeaveRequestCreateDto, LeaveRequest>();
            CreateMap<HolidayCreateDto, Holiday>();
        }
    }
}