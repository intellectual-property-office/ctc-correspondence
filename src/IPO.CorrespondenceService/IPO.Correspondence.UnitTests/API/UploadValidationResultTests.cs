using AwesomeAssertions;
using IPO.Correspondence.Services.Validation;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace IPO.FeeService.UnitTests.API
{
    [TestClass]
    public class UploadValidationResultTests
    {

        [TestMethod]
        public void CreateSuccessValidationResultReturnsValidationResultWithAccepted()
        {
            // Arrange  
            var expectedCode = 202;
            var expectedError = string.Empty;
            var expectedErrorCode = string.Empty;

            // Act  
            var result = UploadValidationResult.CreateSuccessValidationResult();

            // Assert 
            result.Code.Should().Be(expectedCode);
            result.Error.Should().Be(expectedError);
            result.ErrorCode.Should().Be(expectedErrorCode);
        }

        [TestMethod]
        public void CreatePayloadTooLargeValidationResultReturnsPayloadTooLargeAndRightDetails()
        {
            // Arrange
            var sizeLimit = 32;
            var expectedCode = 413;
            var expectedError = $"File size larger than {sizeLimit}";
            var expectedErrorCode = "-01";

            // Act  
            var result = UploadValidationResult.CreatePayloadTooLargeValidationResult(sizeLimit);

            // Assert 
            result.Code.Should().Be(expectedCode);
            result.Error.Should().Be(expectedError);
            result.ErrorCode.Should().Be(expectedErrorCode);
        }

        [TestMethod]
        public void CreateUnsupportedFileTypesValidationResultReturnsUnsupportedMediaTypeAndRightDetails()
        {
            // Arrange
            var acceptedFileExtensions = new List<string>() { ".PDF", ".TXT" };
            var expectedCode = 415;
            var expectedError = $"Unsupported file type, supported media types: {string.Join(", ", acceptedFileExtensions)}";
            var expectedErrorCode = "-02";

            // Act  
            var result = UploadValidationResult.CreateUnsupportedFileTypesValidationResult(acceptedFileExtensions);

            // Assert 
            result.Code.Should().Be(expectedCode);
            result.Error.Should().Be(expectedError);
            result.ErrorCode.Should().Be(expectedErrorCode);
        }
    }
}