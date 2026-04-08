using CourierPortal.Core.DTOs.Admin.Applicants;
using FluentValidation;

namespace CourierPortal.Core.Validators.Admin
{
    public class ApplicantDeleteRequestValidator : AbstractValidator<ApplicantDeleteRequest>
    {

        public ApplicantDeleteRequestValidator()
        {
            SetRules();
        }

        private void SetRules()
        {
            RuleFor(x => x.Reason)
                .NotEmpty().When(x => x.SendEmail);
        }
    }
}
