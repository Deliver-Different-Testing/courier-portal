using System;
using CourierPortal.Core.DTOs.Admin.Infringements;
using FluentValidation;

namespace CourierPortal.Core.Validators.Admin
{
    public class InfringementCreateRequestValidator : AbstractValidator<InfringementCreateRequest>
    {
        public InfringementCreateRequestValidator()
        {
            SetRules();
        }

        private void SetRules()
        {
            RuleFor(x => x.CourierCode)
                .NotEmpty();

            RuleFor(x => x.Severity)
                .InclusiveBetween(1, 10);

            RuleFor(x => x.OccurredOn)
                .NotNull()
                .LessThanOrEqualTo(DateTime.Now);
        }
    }
}
