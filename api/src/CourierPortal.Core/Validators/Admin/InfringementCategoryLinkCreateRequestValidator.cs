using CourierPortal.Core.DTOs.Admin.Infringements;
using FluentValidation;

namespace CourierPortal.Core.Validators.Admin
{
    public class InfringementCategoryLinkCreateRequestValidator : AbstractValidator<InfringementCategoryLinkCreateRequest>
    {
        public InfringementCategoryLinkCreateRequestValidator()
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
