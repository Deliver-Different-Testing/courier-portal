using CourierPortal.Core.DTOs.Admin.Infringements;
using FluentValidation;

namespace CourierPortal.Core.Validators.Admin
{
    public class InfringementCancelRequestValidator : AbstractValidator<InfringementCancelRequest>
    {
        public InfringementCancelRequestValidator()
        {
            SetRules();
        }

        private void SetRules()
        {
            RuleFor(x => x.Reason)
                .NotEmpty();
        }
    }
}
