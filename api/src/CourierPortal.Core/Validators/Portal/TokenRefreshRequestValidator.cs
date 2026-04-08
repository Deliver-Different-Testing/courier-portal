using CourierPortal.Core.DTOs.Portal.Auth;
using FluentValidation;

namespace CourierPortal.Core.Validators.Portal
{
    public class TokenRefreshRequestValidator : AbstractValidator<TokenRefreshRequest>
    {
        public TokenRefreshRequestValidator()
        {
            SetRules();
        }

        private void SetRules()
        {
            RuleFor(x => x.Token).NotEmpty();
            RuleFor(x => x.RefreshToken).NotEmpty();
        }
    }
}
