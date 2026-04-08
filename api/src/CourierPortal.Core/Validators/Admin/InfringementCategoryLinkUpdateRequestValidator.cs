using CourierPortal.Core.DTOs.Admin.Infringements;
using FluentValidation;

namespace CourierPortal.Core.Validators.Admin
{
    public class InfringementCategoryLinkUpdateRequestValidator : AbstractValidator<InfringementCategoryLinkUpdateRequest>
    {
        public InfringementCategoryLinkUpdateRequestValidator()
        {
            SetRules();
        }

        private void SetRules()
        {
            RuleFor(x => x.Name)
                .NotEmpty();

            RuleFor(x => x.Link)
                .NotEmpty();
        }
    }
}
