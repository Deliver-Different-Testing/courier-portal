using System;
using CourierPortal.Core.DTOs.Admin.Schedules;
using FluentValidation;

namespace CourierPortal.Core.Validators.Admin
{
    public class ScheduleCreateDtoValidator : AbstractValidator<ScheduleCreateDto>
    {
        public ScheduleCreateDtoValidator()
        {
            SetRules();
        }

        private void SetRules()
        {
            RuleFor(x => x.BookDate)
                .GreaterThanOrEqualTo(DateTime.Today);

            RuleFor(x => x.StartTime)
                .LessThan(x => x.EndTime)
                .WithMessage("'StartTime' must be less than 'EndTime'");

            RuleFor(x => x)
                .Must(x => x.BookDate.Date > DateTime.Today || (x.BookDate.Date == DateTime.Today && x.StartTime >= DateTime.Now.TimeOfDay))
                .WithMessage("Schedule must start in the future");

            RuleFor(x => x.Name)
                .NotEmpty();

            RuleFor(x => x.Location)
                .NotEmpty();

            RuleFor(x => x.Wanted)
                .GreaterThanOrEqualTo(1);
        }
    }
}
