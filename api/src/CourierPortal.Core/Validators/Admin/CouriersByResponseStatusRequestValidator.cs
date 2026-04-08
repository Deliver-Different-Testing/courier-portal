using CourierPortal.Core.DTOs.Admin.Schedules;
using FluentValidation;

namespace CourierPortal.Core.Validators.Admin
{
    public class CouriersByResponseStatusRequestValidator : AbstractValidator<CouriersByResponseStatusRequest>
    {
        public CouriersByResponseStatusRequestValidator()
        {
            SetRules();
        }

        private void SetRules()
        {
            RuleFor(x => x.StatusId)
                .InclusiveBetween(0, 4);
        }
    }
}
