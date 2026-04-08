using CourierPortal.Core.DTOs.Portal.Applications;
using FluentValidation;

namespace CourierPortal.Core.Validators.Portal
{
    public class EmailVerificationRequestValidator : AbstractValidator<EmailVerificationRequest>
    {
        public EmailVerificationRequestValidator()
        {
            SetRules();
        }

        private void SetRules()
        {

            RuleFor(x => x.Email)
                .NotEmpty()
                .EmailAddress();

            RuleFor(x => x.VerificationCode).NotEmpty();
            
        }
    }
}

