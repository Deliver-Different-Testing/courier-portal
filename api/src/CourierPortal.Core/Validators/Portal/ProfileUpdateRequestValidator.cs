using System;
using System.Diagnostics.Metrics;
using System.Linq;
using CourierPortal.Core.DTOs.Portal.Applications;
using CourierPortal.Core.Utilities;
using FluentValidation;

namespace CourierPortal.Core.Validators.Portal
{
    public class ProfileUpdateRequestValidator : AbstractValidator<ProfileUpdateRequest>
    {
        public ProfileUpdateRequestValidator()
        {
            SetRules();
        }

        private void SetRules()
        {
            RuleFor(x => x.Location)
                .NotNull();

            RuleFor(x => x.FirstName)
                .NotEmpty()
                .MaximumLength(50);

            RuleFor(x => x.Surname)
                .NotEmpty()
                .MaximumLength(50);

            RuleFor(x => x.DateOfBirth)
                .NotNull()
                .LessThanOrEqualTo(DateTime.Now);

            RuleFor(x => x.Mobile)
                .NotEmpty()
                .MaximumLength(50);

            RuleFor(x => x.AddressLine1)
                .MaximumLength(255);

            RuleFor(x => x.AddressLine2)
                .MaximumLength(255);

            RuleFor(x => x.AddressLine3)
                .MaximumLength(255);

            RuleFor(x => x.AddressLine4)
                .NotEmpty()
                .MaximumLength(255);

            RuleFor(x => x.AddressLine5)
                .NotEmpty()
                .MaximumLength(255);

            RuleFor(x => x.AddressLine6)
                .NotEmpty()
                .MaximumLength(255);

            RuleFor(x => x.AddressLine7)
                .NotEmpty()
                .MaximumLength(255);

            RuleFor(x => x.AddressLine8)
                .NotEmpty()
                .MaximumLength(255);

            RuleFor(x => x.DriversLicenceNo)
                .NotEmpty()
                .MaximumLength(50);

            RuleFor(x => x.VehicleRegistrationNo)
                .NotEmpty()
                .MaximumLength(50);

            RuleFor(x => x.VehicleType)
                .NotEmpty()
                .MaximumLength(50);

            RuleFor(x => x.TaxNo)
                .NotEmpty()
                .MaximumLength(50)
                .Must(x => CourierUtility.IsValidTaxNo(x) || CourierUtility.IsValidSocialSecurityNo(x)).WithMessage("Invalid IRD, GST or SSN Number.");
            
            RuleFor(x => x.BankRoutingNumber)
                .MaximumLength(9)
                .Must(x => x == null || CourierUtility.IsValidBankRoutingNumber(x)).WithMessage("Invalid bank routing number.");

            RuleFor(x => x.BankAccountNo)
                .NotEmpty()
                .MinimumLength(8)
                .MaximumLength(50);

            RuleFor(x => x.NextOfKin)
                .NotEmpty()
                .MaximumLength(50);

            RuleFor(x => x.NextOfKinRelationship)
                .NotEmpty()
                .MaximumLength(50);

            RuleFor(x => x.NextOfKinPhone)
                .NotEmpty()
                .MaximumLength(50);

            RuleFor(x => x.NextOfKinAddress)
                .NotEmpty()
                .MaximumLength(100);
        }
    }
}

