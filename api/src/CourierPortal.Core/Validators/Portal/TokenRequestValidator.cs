using CourierPortal.Core.DTOs.Portal.Auth;
using FluentValidation;

namespace CourierPortal.Core.Validators.Portal
{
    public class TokenRequestValidator : AbstractValidator<TokenRequest>
    {
        public TokenRequestValidator()
        {
            SetRules();
        }

        private void SetRules()
        {
            RuleFor(x => x.Username).NotEmpty();
            RuleFor(x => x.Password).NotEmpty();
        }
    }
}
