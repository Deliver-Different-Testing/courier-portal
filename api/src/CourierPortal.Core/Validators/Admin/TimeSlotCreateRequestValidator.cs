using System;
using CourierPortal.Core.DTOs.Admin.Schedules;
using FluentValidation;

namespace CourierPortal.Core.Validators.Admin
{
    public class TimeSlotCreateRequestValidator : AbstractValidator<TimeSlotCreateRequest>
    {
        public TimeSlotCreateRequestValidator()
        {
            SetRules();
        }

        private void SetRules()
        {
            RuleFor(x => x.Location)
                .NotEmpty();

            RuleFor(x => x.BookDateTime)
                .GreaterThan(DateTime.Now).When(x => x.BookDateTime.Kind != DateTimeKind.Utc).WithMessage("Time slot must start not be in the past.")
                .GreaterThan(DateTime.UtcNow).When(x => x.BookDateTime.Kind == DateTimeKind.Utc).WithMessage("Time slot must start not be in the past.");

            RuleFor(x => x.Wanted)
                .GreaterThanOrEqualTo(1).When(x => x.Wanted.HasValue);

            RuleFor(x => x.VehicleTypes)
                .NotNull();
        }

    }

}
