using Microsoft.AspNetCore.Http;
using IPO.Common.Infrastructure;
using IPO.Correspondence.Interfaces;

namespace IPO.Correspondence.Services.Validation
{
    public class PreliminaryFileValidator : IPreliminaryFileValidator
    {
        public int SizeLimit { get; set; }
        public List<string>? AcceptedFileExtensions { get; set; }

        public virtual void Validate(IFormFile file)
        {
            if (file.Length > SizeLimit)            
                ThrowStatusCodeException(UploadValidationResult.CreatePayloadTooLargeValidationResult(SizeLimit));

            if (!AcceptedFileExtensions!.Contains(Path.GetExtension(file.FileName).ToUpperInvariant()))
                ThrowStatusCodeException(UploadValidationResult.CreateUnsupportedFileTypesValidationResult(this.AcceptedFileExtensions));
        }

        public virtual void ValidatePrecompiledLetter(IFormFile file)
        {
            var validFileType = ".PDF";

            if (file.Length > SizeLimit)
                ThrowStatusCodeException(UploadValidationResult.CreatePayloadTooLargeValidationResult(SizeLimit));

            if (!Path.GetExtension(file.FileName).ToUpperInvariant().Contains(validFileType))
                ThrowStatusCodeException(UploadValidationResult.CreatePrecompiledLetterUnsupportedFileTypesValidationResult(validFileType));
        }

        protected virtual void ThrowStatusCodeException(UploadValidationResult result)
        {
            var error = Error.GetError<PreliminaryFileValidator>();
            error.Code += result.ErrorCode;
            error.Description += result.Error;
            throw new StatusCodeException(error, result.Error!, null, result.Code);
        }
    }
}
