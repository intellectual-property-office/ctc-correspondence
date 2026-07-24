using IPO.Correspondence.Interfaces;
using Microsoft.AspNetCore.Http;

namespace IPO.Correspondence.BDDTests.Mocks
{
    public class MockPreliminaryFileValidator : IPreliminaryFileValidator
    {
        public void Validate(IFormFile file) {}
        public void ValidatePrecompiledLetter(IFormFile file) {}
    }
}
