using CourierPortal.Core.DTOs.Admin.Messages;
using FluentValidation;

namespace CourierPortal.Core.Validators.Admin
{
    public class CreateMessagesRequestValidator : AbstractValidator<CreateMessagesRequest>
    {
        public CreateMessagesRequestValidator()
        {
            SetRules();
        }

        private void SetRules()
        {
            RuleFor(x => x.Messages)
                .NotEmpty();

            RuleForEach(x => x.Messages)
                .SetValidator(new CreateMessageDtoValidator());
        }
    }
}
