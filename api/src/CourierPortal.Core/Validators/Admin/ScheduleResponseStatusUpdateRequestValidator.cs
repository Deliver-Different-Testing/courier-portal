using CourierPortal.Core.DTOs.Admin.Schedules;
using FluentValidation;

namespace CourierPortal.Core.Validators.Admin
{
    public class ScheduleResponseStatusUpdateRequestValidator : AbstractValidator<ScheduleResponseStatusUpdateRequest>
    {
        public ScheduleResponseStatusUpdateRequestValidator()
        {
            SetRules();
        }

        private void SetRules()
        {
            RuleFor(x => x.StatusId)
                .Must(x => x == 1 || x == 3)
                .WithMessage("'StatusId' not supported");

            RuleFor(x => x.Ids)
                .NotEmpty();
        }
    }
}
