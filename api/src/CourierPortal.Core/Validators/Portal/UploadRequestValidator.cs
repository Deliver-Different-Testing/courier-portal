using System;
using System.Linq;
using CourierPortal.Core.DTOs.Portal.Applications;
using FluentValidation;

namespace CourierPortal.Core.Validators.Portal
{
    public class UploadRequestValidator : AbstractValidator<UploadRequest>
    {
        private static readonly string[] _supportedContentTypes = { "application/pdf", "image/jpeg", "image/png", "image/bmp" };
        private static readonly string[] _supportedFileExtensions = { ".pdf", ".png", ".jpg", ".jpeg", ".jfif", ".pjpeg", ".pjp", ".bmp", ".dib" };

        public UploadRequestValidator()
        {
            SetRules();
        }

        private void SetRules()
        {
            RuleFor(x => x.DocumentId)
                .GreaterThan(0).WithMessage("'DocumentId' is invalid.");

            RuleFor(x => x.File)
                .NotNull();

            RuleFor(x => x.File)
                .Must(x => !string.IsNullOrWhiteSpace(x.ContentType) && _supportedContentTypes.Contains(x.ContentType.ToLower()) && !string.IsNullOrWhiteSpace(x.FileName) && _supportedFileExtensions.Any(ext => x.FileName.EndsWith(ext, StringComparison.OrdinalIgnoreCase)))
                .WithMessage("'File' has an invalid file type.")
                .When(x => x.File != null)
                .Must(x => x.Length <= 10000000)
                .WithMessage("'File' size must not be greater than 10MB.")
                .When(x => x.File != null);

        }
    }
}
