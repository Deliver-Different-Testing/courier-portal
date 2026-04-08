using System;
using CourierPortal.Core.DTOs.Admin.Schedules;
using FluentValidation;

namespace CourierPortal.Core.Validators.Admin
{
    public class ScheduleCopyRequestValidator : AbstractValidator<ScheduleCopyRequest>
    {
        public ScheduleCopyRequestValidator()
        {
            SetRules();
        }

        private void SetRules()
        {
            RuleFor(x => x.DestinationDate)
                .NotNull()
                .GreaterThanOrEqualTo(DateTime.Today.AddDays(1));

            RuleFor(x => x.SourceDate)
                .NotNull();

            RuleFor(x => x.Locations)
                .NotEmpty();
        }
    }
}
