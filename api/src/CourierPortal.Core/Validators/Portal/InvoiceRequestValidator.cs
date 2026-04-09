using CourierPortal.Core.DTOs.Portal.Invoices;
using CourierPortal.Core.Utilities;
using FluentValidation;

namespace CourierPortal.Core.Validators.Portal
{
    public class InvoiceRequestValidator : AbstractValidator<InvoiceRequest>
    {
        public InvoiceRequestValidator()
        {
            SetRules();
        }

        private void SetRules()
        {
            RuleFor(x => x.InvoiceNo)
                .NotEmpty()
.WithMessage("Invalid format.");
        }
    }
}
