using System.Linq;
using CourierPortal.Core.DTOs.Portal.Couriers;
using CourierPortal.Core.Utilities;
using FluentValidation;

namespace CourierPortal.Core.Validators.Portal
{
    public class CourierUpdateRequestValidator : AbstractValidator<CourierUpdateRequest>
    {
        public CourierUpdateRequestValidator()
        {
            SetRules();
        }

        private void SetRules()
        {
            RuleFor(x => x.FirstName).NotEmpty();
            RuleFor(x => x.Surname).NotEmpty();
            RuleFor(x => x.Mobile).NotEmpty();
            RuleFor(x => x.Email).NotEmpty();
            RuleFor(x => x.AddressLine1)
                .MaximumLength(255);
            RuleFor(x => x.AddressLine2)
                .MaximumLength(255);
            RuleFor(x => x.AddressLine3)
                .MaximumLength(255);
            RuleFor(x => x.AddressLine4)
                .MaximumLength(255)
                .NotEmpty();
            RuleFor(x => x.AddressLine5)
                .MaximumLength(255)
                .NotEmpty();
            RuleFor(x => x.AddressLine6)
                .MaximumLength(255)
                .NotEmpty();
            RuleFor(x => x.AddressLine7)
                .MaximumLength(255)
                .NotEmpty();
            RuleFor(x => x.AddressLine8)
                .MaximumLength(255)
                .NotEmpty();
            RuleFor(x => x.DriversLicenceNo).NotEmpty();
            RuleFor(x => x.VehicleRegistrationNo).NotEmpty();
            RuleFor(x => x.BankRoutingNumber)
                .MaximumLength(9)
                .Must(x => x == null || CourierUtility.IsValidBankRoutingNumber(x)).WithMessage("Invalid bank routing number.");
            RuleFor(x => x.BankAccountNo)
                .NotEmpty()
                .MinimumLength(8)
                .MaximumLength(50);
            RuleFor(x => x.TaxNo)
                .NotEmpty()
                .Must(x => CourierUtility.IsValidTaxNo(x) || CourierUtility.IsValidSocialSecurityNo(x)).WithMessage("Invalid IRD, GST or SSN Number.");
        }


    }
}
