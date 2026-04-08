using CourierPortal.Core.DTOs.Admin.Auth;
using FluentValidation;

namespace CourierPortal.Core.Validators.Admin
{
    public class CourierPortalAccessValidationRequestValidator : AbstractValidator<CourierPortalAccessValidationRequest>
    {
        public CourierPortalAccessValidationRequestValidator()
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
