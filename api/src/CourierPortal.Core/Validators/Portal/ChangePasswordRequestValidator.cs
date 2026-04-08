using System.Linq;
using CourierPortal.Core.DTOs.Portal.Auth;
using FluentValidation;

namespace CourierPortal.Core.Validators.Portal
{
    public class ChangePasswordRequestValidator : AbstractValidator<ChangePasswordRequest>
    {
        public ChangePasswordRequestValidator()
        {
            SetRules();
        }

        private void SetRules()
        {
            RuleFor(x => x.OldPassword)
                .NotEmpty();
            RuleFor(x => x.NewPassword)
                .NotEmpty()
                .MinimumLength(6)
                .MaximumLength(50)
                .Must(x => x.Any(c => "0123456789".IndexOf(c) >= 0)).WithMessage("'New Password' must contain at least one number (0-9).")
                .Must(x => x.Any(c => "abcdefghijklmnopqrstuvwxyz".IndexOf(c) >= 0)).WithMessage("'New Password' must contain at least one lower case letter (a-z).")
                .Must(x => x.Any(c => "ABCDEFGHIJKLMNOPQRSTUVWXYZ".IndexOf(c) >= 0)).WithMessage("'New Password' must contain at least one upper case letter (A-Z).")
                .Must(x => x.Any(c => "0123456789".IndexOf(c) < 0 && "abcdefghijklmnopqrstuvwxyz".IndexOf(char.ToLower(c)) < 0)).WithMessage("'New Password' must contain at least one special character.");

            RuleFor(x => x.ConfirmPassword)
                .NotEmpty()
                .Equal(x => x.NewPassword).WithMessage("'Confirm Password' does not match 'New Password'.");
        }
    }
}
