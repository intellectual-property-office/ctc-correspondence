using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;
using Moq;
using IPO.Common.Infrastructure;
using System.Text.Json;

namespace IPO.Correspondence.UnitTests.API
{

    public class HttpContextFactory
    {
        public static Mock<IFormFile> CreateMockFormFile(string content, string fileName)
        {
            var mockFile = new Mock<IFormFile>();
            var ms = new MemoryStream();
            var writer = new StreamWriter(ms);
            writer.Write(content);
            writer.Flush();
            ms.Position = 0;
            mockFile.Setup(s => s.OpenReadStream()).Returns(ms);
            mockFile.Setup(s => s.FileName).Returns(fileName);
            mockFile.Setup(s => s.Length).Returns(ms.Length);

            return mockFile;
        }

        public static HttpContext CreateHttpContext(object content, IFormFile[] formFiles = null!)
        {
            var httpContext = new DefaultHttpContext();
            var contentDictionary = new Dictionary<string, StringValues>();
            contentDictionary.Add("content", new StringValues(value: IPOJsonSerialization.Serialize(content)));
            var formFilesCollection = new FormFileCollection();

            if (formFiles != null)
                formFilesCollection.AddRange(formFiles);

            httpContext.Request.Form = new FormCollection(contentDictionary, formFilesCollection);

            return httpContext;

        }
    }
}
