using FluentValidation;
using SimpleApi.Models;

namespace SimpleApi.Validators
{
    public class LeaveRequestValidator : AbstractValidator<LeaveRequest>
    {
        public LeaveRequestValidator()
        {
            RuleFor(x => x.UserId).GreaterThan(0).WithMessage("Geçerli bir kullanıcı kimliği giriniz.");
            RuleFor(x => x.StartDate).NotEmpty().WithMessage("Başlangıç tarihi boş olamaz.");
            RuleFor(x => x.EndDate).NotEmpty().WithMessage("Bitiş tarihi boş olamaz.")
                .GreaterThan(x => x.StartDate).WithMessage("Bitiş tarihi, başlangıç tarihinden sonra olmalıdır.");
            RuleFor(x => x.LeaveType).NotEmpty().WithMessage("İzin türü boş olamaz.");
        }
    }
}
