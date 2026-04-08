using System;
using CourierPortal.Core.DTOs.Portal.Applications;
using FluentValidation;

namespace CourierPortal.Core.Validators.Portal
{
    public class DeclarationUpdateRequestValidator : AbstractValidator<DeclarationUpdateRequest>
    {
        public DeclarationUpdateRequestValidator()
        {
            SetRules();
        }

        private void SetRules()
        {
            RuleFor(x => x.Text)
                .NotEmpty();

            RuleFor(x => x.Agree)
                .Must(x => x)
                .WithMessage("Applicant must agree to terms of the declaration.");

            RuleFor(x => x.Name)
                .NotEmpty()
                .MaximumLength(50);

            RuleFor(x => x.Signature).NotEmpty();

            RuleFor(x => x.Signature)
                .Must(x => x.StartsWith("data:", StringComparison.OrdinalIgnoreCase)
                           && x.Contains("image/png;", StringComparison.OrdinalIgnoreCase)
                           && x.Contains("base64,", StringComparison.OrdinalIgnoreCase)
                           && x.IndexOf(';') > 0
                           && x.IndexOf(';') < x.IndexOf(',')
                           && Convert.FromBase64String(x.Substring(x.IndexOf(',') + 1)).Length > 0)
                .WithMessage("Invalid signature")
                .When(x => !string.IsNullOrWhiteSpace(x.Signature));

        }

    }
}

