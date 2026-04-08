using CourierPortal.Core.DTOs.Admin.Messages;
using FluentValidation;

namespace CourierPortal.Core.Validators.Admin
{
    public class CourierMessagesRequestValidator : AbstractValidator<CourierMessagesRequest>
    {

        public CourierMessagesRequestValidator()
        {
            SetRules();
        }

        private void SetRules()
        {
            RuleFor(x => x.Code)
                .NotEmpty();
        }
    }
}
