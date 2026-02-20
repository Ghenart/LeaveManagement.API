using FluentValidation;
using SimpleApi.DTOs;

namespace SimpleApi.Validators
{
    public class UserCreateDtoValidator : AbstractValidator<UserCreateDto>
    {
        public UserCreateDtoValidator()
        {
            RuleFor(x => x.Username).NotEmpty().WithMessage("Kullanıcı adı boş olamaz.")
                .EmailAddress().WithMessage("Geçerli bir e-posta adresi giriniz.");
            RuleFor(x => x.Password).NotEmpty().WithMessage("Şifre boş olamaz.")
                .MinimumLength(6).WithMessage("Şifre en az 6 karakter olmalıdır.");
            RuleFor(x => x.Role).NotEmpty().WithMessage("Rol boş olamaz.");
            RuleFor(x => x.LeaveBalance).GreaterThanOrEqualTo(0).WithMessage("İzin bakiyesi 0 veya daha büyük olmalıdır.");
        }
    }
}
