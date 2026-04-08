using CourierPortal.Core.DTOs.Admin.Infringements;
using FluentValidation;

namespace CourierPortal.Core.Validators.Admin
{
    public class InfringementCategoryUpdateRequestValidator : AbstractValidator<InfringementCategoryUpdateRequest>
    {
        public InfringementCategoryUpdateRequestValidator()
        {
            SetRules();
        }

        private void SetRules()
        {
            RuleFor(x => x.Severity)
                .InclusiveBetween(1, 10);
        }
    }
}
