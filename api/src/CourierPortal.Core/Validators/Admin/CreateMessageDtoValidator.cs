using CourierPortal.Core.DTOs.Admin.Messages;
using FluentValidation;

namespace CourierPortal.Core.Validators.Admin
{
    public class CreateMessageDtoValidator : AbstractValidator<CreateMessageDto>
    {
        public CreateMessageDtoValidator()
        {
            SetRules();
        }

        private void SetRules()
        {
            RuleFor(x => x.CourierId)
                .GreaterThan(0);

            RuleFor(x => x.Type)
                .Must(x => x == 1 || x == 2 || x == 3).WithMessage("Invalid Message Type.");

            RuleFor(x => x.Message)
                .NotEmpty()
                .MaximumLength(160);

        }
    }
}
