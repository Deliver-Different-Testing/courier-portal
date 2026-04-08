using CourierPortal.Core.DTOs.Portal.Runs;
using FluentValidation;

namespace CourierPortal.Core.Validators.Portal
{
    public class EnquiryRequestValidator : AbstractValidator<EnquiryRequest>
    {
        public EnquiryRequestValidator()
        {
            SetRules();
        }

        private void SetRules()
        {
            RuleFor(x => x.JobNumber)
                .NotEmpty();

            RuleFor(x => x.Message)
                .NotEmpty();
        }
    }
}
