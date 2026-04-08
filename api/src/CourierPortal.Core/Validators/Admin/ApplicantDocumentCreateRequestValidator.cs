using System;
using System.Linq;
using CourierPortal.Core.DTOs.Admin.Applicants;
using FluentValidation;

namespace CourierPortal.Core.Validators.Admin
{
    public class ApplicantDocumentCreateRequestValidator : AbstractValidator<ApplicantDocumentCreateRequest>
    {

        private static readonly string[] _acceptedFileTypes = { "application/pdf", "image/jpeg", "image/png", "image/bmp" };
        private static readonly string[] _supportedFileExtensions = { ".pdf", ".png", ".jpg", ".jpeg", ".jfif", ".pjpeg", ".pjp", ".bmp", ".dib" };

        public ApplicantDocumentCreateRequestValidator()
        {
            SetRules();
        }

        private void SetRules()
        {
            RuleFor(x => x.Name)
                .NotEmpty()
                .MaximumLength(50);

            RuleFor(x => x.File)
                .Must(x => !string.IsNullOrWhiteSpace(x.FileName) && _supportedFileExtensions.Any(ext => x.FileName.EndsWith(ext, StringComparison.OrdinalIgnoreCase)))
                .WithMessage("'File' is invalid.")
                .When(x => x.File != null);

            RuleFor(x => x.File)
                .Must(x => !string.IsNullOrWhiteSpace(x.DataUrl) 
                           && x.DataUrl.StartsWith("data:", StringComparison.OrdinalIgnoreCase)
                           && x.DataUrl.Contains("base64,", StringComparison.OrdinalIgnoreCase)
                           && x.DataUrl.IndexOf(';') > 0
                           && x.DataUrl.IndexOf(';') < x.DataUrl.IndexOf(',')
                           && x.DataUrl.IndexOf(',') < x.DataUrl.Length - 1
                           && _acceptedFileTypes.Contains(x.DataUrl.Substring(x.DataUrl.IndexOf(':') + 1, x.DataUrl.IndexOf(';') - x.DataUrl.IndexOf(':') - 1)))
                .WithMessage("Invalid file")
                .When(x => x.File != null);
        }
    }
}
