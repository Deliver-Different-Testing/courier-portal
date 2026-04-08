using System.Linq;
using CourierPortal.Core.DTOs.Portal.Applications;
using FluentValidation;

namespace CourierPortal.Core.Validators.Portal
{
    public class RegisterRequestValidator : AbstractValidator<RegisterRequest>
    {
        public RegisterRequestValidator()
        {
            SetRules();
        }

        private void SetRules()
        {
            RuleFor(x => x.Location)
                .NotNull();

            RuleFor(x => x.VehicleType)
                .NotEmpty()
                .MaximumLength(50);

            RuleFor(x => x.FirstName)
                .NotEmpty()
                .MaximumLength(50);

            RuleFor(x => x.Surname)
                .NotEmpty()
                .MaximumLength(50);

            RuleFor(x => x.Phone)
                .MaximumLength(50);

            RuleFor(x => x.Mobile)
                .NotEmpty()
                .MaximumLength(50);

            RuleFor(x => x.Email)
                .NotEmpty()
                .MaximumLength(50)
                .EmailAddress();

            //RuleFor(x => x.Email)
            //    .Must(x => Regex.IsMatch(x, @"^((([a-z]|\d|[!#\$%&'\*\+\-\/=\?\^_`{\|}~]|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])+(\.([a-z]|\d|[!#\$%&'\*\+\-\/=\?\^_`{\|}~]|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])+)*)|((\x22)((((\x20|\x09)*(\x0d\x0a))?(\x20|\x09)+)?(([\x01-\x08\x0b\x0c\x0e-\x1f\x7f]|\x21|[\x23-\x5b]|[\x5d-\x7e]|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])|(\\([\x01-\x09\x0b\x0c\x0d-\x7f]|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF]))))*(((\x20|\x09)*(\x0d\x0a))?(\x20|\x09)+)?(\x22)))@((([a-z]|\d|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])|(([a-z]|\d|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])([a-z]|\d|-|\.|_|~|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])*([a-z]|\d|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])))\.)+(([a-z]|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])|(([a-z]|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])([a-z]|\d|-|\.|_|~|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])*([a-z]|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])))\.?$"))
            //    .WithMessage("'Email' is invalid")
            //    .When(x => !string.IsNullOrWhiteSpace(x.Email));

            RuleFor(x => x.NewPassword)
                .NotEmpty()
                .MinimumLength(6)
                .MaximumLength(50);

            RuleFor(x => x.NewPassword)
                .Must(x => x.Any(c => "0123456789".IndexOf(c) >= 0)).WithMessage("'New Password' must contain at least one number (0-9).")
                .Must(x => x.Any(c => "abcdefghijklmnopqrstuvwxyz".IndexOf(c) >= 0)).WithMessage("'New Password' must contain at least one lower case letter (a-z).")
                .Must(x => x.Any(c => "ABCDEFGHIJKLMNOPQRSTUVWXYZ".IndexOf(c) >= 0)).WithMessage("'New Password' must contain at least one upper case letter (A-Z).")
                .Must(x => x.Any(c => "0123456789".IndexOf(c) < 0 && "abcdefghijklmnopqrstuvwxyz".IndexOf(char.ToLower(c)) < 0)).WithMessage("'New Password' must contain at least one special character.")
                .When(x => x.NewPassword != null);

            RuleFor(x => x.ConfirmPassword)
                .NotEmpty()
                .Equal(x => x.NewPassword).WithMessage("'Confirm Password' does not match 'New Password'.");
        }
    }
}
