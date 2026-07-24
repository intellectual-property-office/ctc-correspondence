using Microsoft.AspNetCore.Http;

namespace IPO.Correspondence.Interfaces
{
    public interface IPreliminaryFileValidator
    {
        void Validate(IFormFile file);
        void ValidatePrecompiledLetter(IFormFile file);
    }
}