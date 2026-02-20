using FluentValidation;
using SimpleApi.DTOs;

namespace SimpleApi.Validators
{
    public class LeaveRequestCreateDtoValidator : AbstractValidator<LeaveRequestCreateDto>
    {
        public LeaveRequestCreateDtoValidator()
        {
            RuleFor(x => x.StartDate).NotEmpty().WithMessage("Başlangıç tarihi boş olamaz.");
            RuleFor(x => x.EndDate).NotEmpty().WithMessage("Bitiş tarihi boş olamaz.")
                .GreaterThan(x => x.StartDate).WithMessage("Bitiş tarihi, başlangıç tarihinden sonra olmalıdır.");
            RuleFor(x => x.LeaveType).NotEmpty().WithMessage("İzin türü boş olamaz.");
        }
    }
}
