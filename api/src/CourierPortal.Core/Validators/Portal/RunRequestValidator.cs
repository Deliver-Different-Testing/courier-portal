using CourierPortal.Core.DTOs.Portal.Runs;
using FluentValidation;

namespace CourierPortal.Core.Validators.Portal
{
    public class RunRequestValidator : AbstractValidator<RunRequest>
    {
        public RunRequestValidator()
        {
            SetRules();
        }

        private void SetRules()
        {
            RuleFor(x => x.BookDate)
                .NotEmpty();
            RuleFor(x => x.RunName)
                .NotEmpty();
        }
    }
}
