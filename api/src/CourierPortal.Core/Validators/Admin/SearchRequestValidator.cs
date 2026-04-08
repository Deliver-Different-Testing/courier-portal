using CourierPortal.Core.DTOs.Admin.Common;
using FluentValidation;

namespace CourierPortal.Core.Validators.Admin
{
    public class SearchRequestValidator : AbstractValidator<SearchRequest>
    {
        public SearchRequestValidator()
        {
            SetRules();
        }

        private void SetRules()
        {
            RuleFor(x => x.SearchText).NotNull();
        }

    }
}
