using CourierPortal.Core.DTOs.Admin.Common;
using FluentValidation;

namespace CourierPortal.Core.Validators.Admin
{
    public class IdentifierRequestValidator : AbstractValidator<IdentifierRequest>
    {
        public IdentifierRequestValidator()
        {
            SetRules();
        }

        private void SetRules()
        {
            //RuleFor(x => x.Id).GreaterThan(0);
            RuleFor(x => x.Id)
                .GreaterThan(0);
        }
    }
}
