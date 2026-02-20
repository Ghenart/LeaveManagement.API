using FluentValidation;
using SimpleApi.DTOs;
using System;

namespace SimpleApi.Validators
{
    public class HolidayCreateDtoValidator : AbstractValidator<HolidayCreateDto>
    {
        public HolidayCreateDtoValidator()
        {
            RuleFor(x => x.Name).NotEmpty().WithMessage("Tatil adı boş olamaz.");
            RuleFor(x => x.Date).NotEmpty().WithMessage("Tarih boş olamaz.");
        }
    }
}
