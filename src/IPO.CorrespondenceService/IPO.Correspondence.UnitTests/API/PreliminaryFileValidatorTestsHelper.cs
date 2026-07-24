using IPO.Correspondence.Services.Validation;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace IPO.Correspondence.UnitTests.API
{
    [TestClass]
    [TestCategory("PreliminaryFileValidatorTests")]
    public class PreliminaryFileValidatorTestsHelper : PreliminaryFileValidator
    {
        public void TestThrowStatusCodeException(UploadValidationResult result)
        {
            this.ThrowStatusCodeException(result);
        }
    }
}
