using FluentValidation;
using SimpleApi.DTOs;

namespace SimpleApi.Validators
{
    public class ChangePasswordDtoValidator : AbstractValidator<ChangePasswordDto>
    {
        public ChangePasswordDtoValidator()
        {
            RuleFor(x => x.OldPassword).NotEmpty().WithMessage("Eski şifre boş olamaz.");
            RuleFor(x => x.NewPassword).NotEmpty().WithMessage("Yeni şifre boş olamaz.")
                .MinimumLength(6).WithMessage("Yeni şifre en az 6 karakter olmalıdır.");
        }
    }
}
