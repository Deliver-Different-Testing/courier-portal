using CourierPortal.Core.DTOs.Portal.Runs;
using FluentValidation;

namespace CourierPortal.Core.Validators.Portal
{
    public class AvailableRunRequestValidator : AbstractValidator<AvailableRunRequest>
    {
        public AvailableRunRequestValidator()
        {
            SetRules();
        }

        private void SetRules()
        {
            RuleFor(x => x.Id)
                .NotEmpty()
                .GreaterThan(0);
        }
    }
}
