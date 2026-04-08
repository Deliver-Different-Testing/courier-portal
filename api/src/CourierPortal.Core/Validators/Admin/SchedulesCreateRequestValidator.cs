using CourierPortal.Core.DTOs.Admin.Schedules;
using FluentValidation;

namespace CourierPortal.Core.Validators.Admin
{
    public class SchedulesCreateRequestValidator : AbstractValidator<SchedulesCreateRequest>
    {
        public SchedulesCreateRequestValidator()
        {
            SetRules();
        }

        private void SetRules()
        {
            RuleFor(x => x.Schedules)
                .NotEmpty();

            RuleForEach(x => x.Schedules)
                .SetValidator(new ScheduleCreateDtoValidator());
        }

    }

}
