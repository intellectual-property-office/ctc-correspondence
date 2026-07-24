using Microsoft.AspNetCore.Http;
using System.Collections.Generic;

namespace IPO.Correspondence.Services.Validation
{
    public class UploadValidationResult
    {
        public int Code { get; set; }
        public string? Error { get; set; }
        public string? ErrorCode { get; set; }

        public static UploadValidationResult CreateSuccessValidationResult()
        {
            return new UploadValidationResult()
            {
                Code = StatusCodes.Status202Accepted,
                Error = string.Empty,
                ErrorCode = string.Empty
            };
        }

        public static UploadValidationResult CreatePayloadTooLargeValidationResult(int sizeLimit)
        {
            return new UploadValidationResult()
            {
                Code = 413,
                Error = $"File size larger than {sizeLimit}",
                ErrorCode = "-01"
            };
        }

        public static UploadValidationResult CreateUnsupportedFileTypesValidationResult(IEnumerable<string> acceptedFileExtensions)
        {
            return new UploadValidationResult()
            {
                Code = 415,
                Error = $"Unsupported file type, supported media types: {string.Join(", ", acceptedFileExtensions)}",
                ErrorCode = "-02"
            };
        }

        public static UploadValidationResult CreatePrecompiledLetterUnsupportedFileTypesValidationResult(string validFileType)
        {
            return new UploadValidationResult()
            {
                Code = 415,
                Error = $" Unsupported file type, supported media type: {validFileType}",
                ErrorCode = "-03"
            };
        }

    }
}
