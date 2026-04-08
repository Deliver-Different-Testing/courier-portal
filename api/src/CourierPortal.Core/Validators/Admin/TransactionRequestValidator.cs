using CourierPortal.Core.DTOs.Admin.Invoices;
using CourierPortal.Core.Core.Application.Utilities;
using FluentValidation;

namespace CourierPortal.Core.Validators.Admin
{
    public class TransactionRequestValidator : AbstractValidator<TransactionRequest>
    {
        public TransactionRequestValidator()
        {
            SetRules();
        }

        private void SetRules()
        {
            RuleFor(x => x.Number)
                .NotEmpty()
                .Must(x => InvoiceUtility.IsValidNumber(x) || DeductionUtility.IsValidNumber(x)).WithMessage("Invalid Number.");
        }
    }
}
