using CourierPortal.Core.DTOs.Admin.Applicants;
using FluentValidation;

namespace CourierPortal.Core.Validators.Admin
{
    public class ApplicantApproveRequestValidator : AbstractValidator<ApplicantApproveRequest>
    {

        public ApplicantApproveRequestValidator()
        {
            SetRules();
        }

        private void SetRules()
        {
            RuleFor(x => x.CourierTypeId)
                .GreaterThanOrEqualTo(1);

            RuleFor(x => x.MasterCourierId)
                .NotNull()
                .GreaterThanOrEqualTo(1)
                .When(x => x.CourierTypeId == 3);

            RuleFor(x => x.Code)
                .NotEmpty()
                .MaximumLength(50);

            RuleFor(x => x.Fleet)
                .NotEmpty();

            RuleFor(x => x.Code)
                .Must(x => int.TryParse(x, out int result) && result >= 3000 && result < 9999)
                .WithMessage("Invalid Courier Code")
                .When(x => !string.IsNullOrEmpty(x.Code));
        }
    }
}
