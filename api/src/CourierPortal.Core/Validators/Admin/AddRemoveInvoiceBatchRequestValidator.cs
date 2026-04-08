using CourierPortal.Core.DTOs.Admin.Invoices;
using CourierPortal.Core.Core.Application.Utilities;
using FluentValidation;

namespace CourierPortal.Core.Validators.Admin
{
    public class AddRemoveInvoiceBatchRequestValidator : AbstractValidator<AddRemoveInvoiceBatchRequest>
    {
        public AddRemoveInvoiceBatchRequestValidator()
        {
            SetRules();
        }

        private void SetRules()
        {
            RuleFor(x => x.InvoiceNos)
                .NotEmpty()
                .Must(x => !x.Exists(i=> !InvoiceUtility.IsValidNumber(i))).WithMessage("Invalid Invoice No.");
        }
    }
}
