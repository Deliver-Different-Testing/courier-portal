using CourierPortal.Core.DTOs.Admin.Invoices;
using CourierPortal.Core.Core.Application.Utilities;
using FluentValidation;

namespace CourierPortal.Core.Validators.Admin
{
    public class InvoiceBatchRequestValidator : AbstractValidator<InvoiceBatchRequest>
    {
        public InvoiceBatchRequestValidator()
        {
            SetRules();
        }

        private void SetRules()
        {
            RuleFor(x => x.BatchNo)
                .NotEmpty()
                .Must(InvoiceBatchUtility.IsValidBatchNo).WithMessage("Invalid Batch No.");
        }
    }
}
