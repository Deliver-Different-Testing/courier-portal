using System;
using System.Linq;
using CourierPortal.Core.DTOs.Admin.Contracts;
using FluentValidation;

namespace CourierPortal.Core.Validators.Admin
{
    public class ContractCreateRequestValidator : AbstractValidator<ContractCreateRequest>
    {

        private static readonly string[] _supportedFileTypes = { "application/pdf" };
        private static readonly string[] _supportedFileExtensions = { ".pdf" };

        public ContractCreateRequestValidator()
        {
            SetRules();
        }

        private void SetRules()
        {
            RuleFor(x => x.File)
                .NotNull();

            RuleFor(x => x.File)
                .Must(x => !string.IsNullOrWhiteSpace(x.FileName))
                .WithMessage("'FileName' must not be empty.")
                .When(x => x.File != null);

            RuleFor(x => x.File)
                .Must(x => !string.IsNullOrWhiteSpace(x.FileName)
                           && _supportedFileExtensions.Any(ext => x.FileName.EndsWith(ext, StringComparison.OrdinalIgnoreCase))
                           && !string.IsNullOrWhiteSpace(x.DataUrl) 
                           && x.DataUrl.StartsWith("data:", StringComparison.OrdinalIgnoreCase)
                           && x.DataUrl.Contains("base64,", StringComparison.OrdinalIgnoreCase)
                           && x.DataUrl.IndexOf(';') > 0
                           && x.DataUrl.IndexOf(';') < x.DataUrl.IndexOf(',')
                           && x.DataUrl.IndexOf(',') < x.DataUrl.Length - 1
                           && _supportedFileTypes.Contains(x.DataUrl.Substring(x.DataUrl.IndexOf(':') + 1, x.DataUrl.IndexOf(';') - x.DataUrl.IndexOf(':') - 1)))
                .WithMessage("Invalid file")
                .When(x => x.File != null);
        }
    }
}
