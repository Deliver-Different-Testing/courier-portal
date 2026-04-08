using CourierPortal.Core.DTOs.Admin.Schedules;
using FluentValidation;

namespace CourierPortal.Core.Validators.Admin
{
    public class NotificationsRequestValidator : AbstractValidator<NotificationsRequest>
    {
        public NotificationsRequestValidator()
        {
            SetRules();
        }

        private void SetRules()
        {
            RuleFor(x => x.Ids).NotEmpty();
        }
    }
}
