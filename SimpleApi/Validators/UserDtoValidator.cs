using FluentValidation;
using SimpleApi.DTOs;

namespace SimpleApi.Validators
{
    public class UserDtoValidator : AbstractValidator<UserDto>
    {
        public UserDtoValidator()
        {
            // .EmailAddress() kuralını kaldırdık, artık 'admin' gibi isimleri kabul eder.
            RuleFor(x => x.Username)
                .NotEmpty().WithMessage("Kullanıcı adı boş olamaz.");

            RuleFor(x => x.Password)
                .NotEmpty().WithMessage("Şifre boş olamaz.")
                .MinimumLength(3).WithMessage("Şifre en az 6 karakter olmalıdır.");
        }
    }
}