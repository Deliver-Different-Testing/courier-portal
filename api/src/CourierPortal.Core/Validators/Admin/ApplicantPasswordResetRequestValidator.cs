using CourierPortal.Core.DTOs.Admin.Applicants;
using FluentValidation;

namespace CourierPortal.Core.Validators.Admin
{
    public class ApplicantPasswordResetRequestValidator : AbstractValidator<ApplicantPasswordResetRequest>
    {

        public ApplicantPasswordResetRequestValidator()
        {
            SetRules();
        }

        private void SetRules()
        {
            RuleFor(x => x.NewPassword)
                .NotEmpty()
                .MinimumLength(8);
        }
    }
}
