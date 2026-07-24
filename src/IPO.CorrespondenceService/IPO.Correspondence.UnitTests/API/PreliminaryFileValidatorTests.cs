using AwesomeAssertions;
using IPO.Common.Infrastructure;
using IPO.Correspondence.Services.Validation;
using IPO.Correspondence.UnitTests.API;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace IPO.FeeService.UnitTests.API
{
    [TestClass]
    [TestCategory("PreliminaryFileValidatorTests")]
    public class PreliminaryFileValidatorTests
    {
        private readonly PreliminaryFileValidator _preliminaryFileValidator;

        public PreliminaryFileValidatorTests()
        {
            Error.Add(Error.Create<PreliminaryFileValidator>("E-003"));
            this._preliminaryFileValidator = new PreliminaryFileValidator();
        }

        [TestMethod]
        public void ValidateReturnsSuccess()
        {
            // Arrange  
            var content = Guid.NewGuid().ToString();
            var fileExtension = ".TXT";
            var fileName = $"test{fileExtension}";
            var mockFormFile = HttpContextFactory.CreateMockFormFile(content, fileName);
            this._preliminaryFileValidator.SizeLimit = (content.Length + 1);
            this._preliminaryFileValidator.AcceptedFileExtensions = new List<string>() { fileExtension };

            // Act  
            var resultAction = this._preliminaryFileValidator.Invoking(o => o.Validate(mockFormFile.Object));

            // Assert
            resultAction.Should().NotThrow();
        }

        [TestMethod]
        public void ValidateWhenFileLengthExceedsSizeLimitThrowsPayloadTooLargeException()
        {
            // Arrange  
            var content = Guid.NewGuid().ToString();
            var fileExtension = ".TXT";
            var fileName = $"test{fileExtension}";
            var mockFormFile = HttpContextFactory.CreateMockFormFile(content, fileName);
            this._preliminaryFileValidator.SizeLimit = (content.Length - 1);
            this._preliminaryFileValidator.AcceptedFileExtensions = new List<string>() { fileExtension };
            var expectedExceptionResult = UploadValidationResult.CreatePayloadTooLargeValidationResult(this._preliminaryFileValidator.SizeLimit);

            // Act  
            var resultAction = this._preliminaryFileValidator.Invoking(o => o.Validate(mockFormFile.Object));

            // Assert
            resultAction.Should().ThrowExactly<StatusCodeException>()
            .Which.StatusCode.Should().Be(expectedExceptionResult.Code);
            resultAction.Should().ThrowExactly<StatusCodeException>()
            .Which.Message.Should().Be(expectedExceptionResult.Error);
        }

        [TestMethod]
        public void ValidateWhenTypeIsNotSupportedThrowsUnsupportedFileTypesException()
        {
            // Arrange  
            var content = Guid.NewGuid().ToString();
            var fileExtension = ".TXT";
            var fileName = $"test{fileExtension}";
            var mockFormFile = HttpContextFactory.CreateMockFormFile(content, fileName);
            this._preliminaryFileValidator.SizeLimit = (content.Length + 1);
            this._preliminaryFileValidator.AcceptedFileExtensions = new List<string>() { ".PDF" };
            var expectedExceptionResult = UploadValidationResult.CreateUnsupportedFileTypesValidationResult(this._preliminaryFileValidator.AcceptedFileExtensions);

            // Act  
            var resultAction = this._preliminaryFileValidator.Invoking(o => o.Validate(mockFormFile.Object));

            // Assert
            resultAction.Should().ThrowExactly<StatusCodeException>()
            .Which.StatusCode.Should().Be(expectedExceptionResult.Code);
            resultAction.Should().ThrowExactly<StatusCodeException>()
            .Which.Message.Should().Be(expectedExceptionResult.Error);
        }

        [TestMethod]
        public void ValidatePrecompiledLetterReturnsSuccess()
        {
            // Arrange  
            var content = Guid.NewGuid().ToString();
            var fileExtension = ".PDF";
            var fileName = $"test{fileExtension}";
            var mockFormFile = HttpContextFactory.CreateMockFormFile(content, fileName);
            this._preliminaryFileValidator.SizeLimit = (content.Length + 1);
            this._preliminaryFileValidator.AcceptedFileExtensions = new List<string>() { fileExtension };

            // Act  
            var resultAction = this._preliminaryFileValidator.Invoking(o => o.ValidatePrecompiledLetter(mockFormFile.Object));

            // Assert
            resultAction.Should().NotThrow();
        }

        [TestMethod]
        [ExpectedException(typeof(StatusCodeException))]
        public void ValidatePrecompiledLetterWithWrongFileTypeThrowsException()
        {

            // Arrange  
            var content = Guid.NewGuid().ToString();
            var fileExtension = ".DOCX";
            var fileName = $"test{fileExtension}";
            var mockFormFile = HttpContextFactory.CreateMockFormFile(content, fileName);
            this._preliminaryFileValidator.SizeLimit = (content.Length + 1);
            this._preliminaryFileValidator.AcceptedFileExtensions = new List<string>() { fileExtension };

            // Act  and Assert
            _preliminaryFileValidator.ValidatePrecompiledLetter(mockFormFile.Object); 

        }

        [TestMethod]
        public void ThrowStatusCodeExceptionThrowsStatusCodeExceptionWithRightDetails()
        {
            // Arrange 
            var expectedExceptionResult = UploadValidationResult.CreateSuccessValidationResult();

            // Act 
            int statusCode = 0;
            bool wasStatusCodeExceptionThrown = false;
            try
            {
                new PreliminaryFileValidatorTestsHelper().TestThrowStatusCodeException(expectedExceptionResult);
            }
            catch (StatusCodeException exception)
            {
                wasStatusCodeExceptionThrown = true;
                statusCode = exception.StatusCode;
            }

            // Assert
            wasStatusCodeExceptionThrown.Should().BeTrue();
            statusCode.Should().Be(expectedExceptionResult.Code);
        }
    }
}