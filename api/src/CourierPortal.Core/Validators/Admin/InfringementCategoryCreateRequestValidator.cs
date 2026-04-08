using CourierPortal.Core.DTOs.Admin.Infringements;
using FluentValidation;

namespace CourierPortal.Core.Validators.Admin
{
    public class InfringementCategoryCreateRequestValidator : AbstractValidator<InfringementCategoryCreateRequest>
    {
        public InfringementCategoryCreateRequestValidator()
        {
            SetRules();
        }

        private void SetRules()
        {
            RuleFor(x => x.Name)
                .NotEmpty();

            RuleFor(x => x.Severity)
                .InclusiveBetween(1, 10);
        }
    }
}
