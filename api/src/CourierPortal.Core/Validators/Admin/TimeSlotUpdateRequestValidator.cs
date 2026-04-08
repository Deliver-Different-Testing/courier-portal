using System;
using CourierPortal.Core.DTOs.Admin.Schedules;
using FluentValidation;

namespace CourierPortal.Core.Validators.Admin
{
    public class TimeSlotUpdateRequestValidator : AbstractValidator<TimeSlotUpdateRequest>
    {
        public TimeSlotUpdateRequestValidator()
        {
            SetRules();
        }

        private void SetRules()
        {
            RuleFor(x => x.BookDateTime)
                .GreaterThan(DateTime.Now).When(x => x.BookDateTime.Kind != DateTimeKind.Utc).WithMessage("Time slot must start not be in the past.")
                .GreaterThan(DateTime.UtcNow).When(x => x.BookDateTime.Kind == DateTimeKind.Utc).WithMessage("Time slot must start not be in the past.");
        }

    }

}
