using CourierPortal.Core.DTOs.Portal.Invoices;
using FluentValidation;

namespace CourierPortal.Core.Validators.Portal
{
    public class InvoiceCreateRequestValidator : AbstractValidator<InvoiceCreateRequest>
    {
        public InvoiceCreateRequestValidator()
        {
            SetRules();
        }

        private void SetRules()
        {
            RuleFor(x => x.Runs)
                .NotNull();
        }
    }
}
