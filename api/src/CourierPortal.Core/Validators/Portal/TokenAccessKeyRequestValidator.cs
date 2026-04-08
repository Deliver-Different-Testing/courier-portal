using CourierPortal.Core.DTOs.Portal.Auth;
using FluentValidation;

namespace CourierPortal.Core.Validators.Portal
{
    public class TokenAccessKeyRequestValidator : AbstractValidator<TokenAccessKeyRequest>
    {
        public TokenAccessKeyRequestValidator()
        {
            SetRules();
        }

        private void SetRules()
        {
            RuleFor(x => x.AccessKey)
                .NotEmpty();
        }
    }
}
