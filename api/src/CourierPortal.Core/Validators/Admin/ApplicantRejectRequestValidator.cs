using CourierPortal.Core.DTOs.Admin.Applicants;
using FluentValidation;

namespace CourierPortal.Core.Validators.Admin
{
    public class ApplicantRejectRequestValidator : AbstractValidator<ApplicantRejectRequest>
    {

        public ApplicantRejectRequestValidator()
        {
            SetRules();
        }

        private void SetRules()
        {
            RuleFor(x => x.Reason)
                .NotEmpty();

            RuleFor(x => x.ClearDocuments)
                .NotNull();
        }
    }
}
