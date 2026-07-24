using Azure;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using AwesomeAssertions;
using IPO.Common.Infrastructure;
using IPO.Correspondence.Gateways;
using IPO.ServiceRequest.Models.Configuration;
using Microsoft.Extensions.Options;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace IPO.Correspondence.UnitTests.Gateways
{
    [TestClass]
    public class BlobStorageGatewayTests
    {
        [TestMethod]
        public async Task GetFileStreamAsyncSuccess()
        {
            // Arrange
            var settings = new Settings { ContainerName = "A Container", Topic = "A Topic" };
            var optionsMonitor = Mock.Of<IOptionsMonitor<Settings>>(_ => _.CurrentValue == settings);
            var mockLogger = new Mock<Microsoft.Extensions.Logging.ILogger<BlobStorageGateway>>();
            var blobServiceClientMock = new Mock<BlobServiceClient>(MockBehavior.Strict);
            var blobContainerClientMock = new Mock<BlobContainerClient>(MockBehavior.Strict);
            var blobClientMock = new Mock<BlobClient>(MockBehavior.Strict);

            blobClientMock.Setup(b => b.DownloadToAsync(It.IsAny<Stream>()))
                .ReturnsAsync(new MockResponse() as Response);

            blobContainerClientMock.Setup(b => b.GetBlobClient(It.IsAny<string>()))
                .Returns(blobClientMock.Object);

            blobServiceClientMock.Setup(
                b => b.GetBlobContainerClient(It.IsAny<string>()))
                .Returns(blobContainerClientMock.Object);

            var sut = new BlobStorageGateway(blobServiceClientMock.Object, optionsMonitor, mockLogger.Object);

            // Act
            var result = await sut.GetFileStreamAsync("fileId");

            // Assert
            result.Should().NotBeNull();
            result.Position.Should().Be(0);
            result.Should().BeOfType<MemoryStream>();
        }

        [TestMethod]
        public async Task UploadAttachmentAsyncSuccess()
        {
            // Arrange
            var settings = new Settings { ContainerName = "A Container", Topic = "A Topic" };
            var optionsMonitor = Mock.Of<IOptionsMonitor<Settings>>(_ => _.CurrentValue == settings);
            var mockLogger = new Mock<Microsoft.Extensions.Logging.ILogger<BlobStorageGateway>>();
            var blobServiceClientMock = new Mock<BlobServiceClient>(MockBehavior.Strict);
            var blobContainerClientMock = new Mock<BlobContainerClient>(MockBehavior.Strict);
            var blobClientMock = new Mock<BlobClient>(MockBehavior.Strict);
            var blobContentInfo = new Mock<BlobContentInfo>(MockBehavior.Strict);

            blobClientMock.Setup(b => b.UploadAsync(It.IsAny<Stream>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new MockResponse<BlobContentInfo>(blobContentInfo.Object));

            blobContainerClientMock.Setup(b => b.GetBlobClient(It.IsAny<string>()))
                .Returns(blobClientMock.Object);

            blobServiceClientMock.Setup(
                b => b.GetBlobContainerClient(It.IsAny<string>()))
                .Returns(blobContainerClientMock.Object);

            // Act
            var sut = new BlobStorageGateway(blobServiceClientMock.Object, optionsMonitor, mockLogger.Object);

            using var stream = new MemoryStream();
            var result = await sut.UploadAttachmentAsync(stream);

            // Assert
            result.Should().NotBeNull();
            Guid.TryParse(result, out _).Should().BeTrue();
        }


        [TestMethod]
        public void UploadAttachmentAsyncAndCausesAnException()
        {
            // Arrange
            Error.Add<BlobStorageGateway>("Simulated Error");
            var settings = new Settings { ContainerName = "A Container", Topic = "A Topic" };
            var optionsMonitor = Mock.Of<IOptionsMonitor<Settings>>(_ => _.CurrentValue == settings);
            var mockLogger = new Mock<Microsoft.Extensions.Logging.ILogger<BlobStorageGateway>>();
            var blobServiceClientMock = new Mock<BlobServiceClient>(MockBehavior.Strict);
            var blobContainerClientMock = new Mock<BlobContainerClient>(MockBehavior.Strict);
            var blobClientMock = new Mock<BlobClient>(MockBehavior.Strict);

            blobClientMock.Setup(b => b.UploadAsync(It.IsAny<Stream>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()))
                .Throws(new RequestFailedException(new MockResponse()));

            blobContainerClientMock.Setup(b => b.GetBlobClient(It.IsAny<string>()))
                .Returns(blobClientMock.Object);

            blobServiceClientMock.Setup(
                b => b.GetBlobContainerClient(It.IsAny<string>()))
                .Returns(blobContainerClientMock.Object);

            // Act
            var sut = new BlobStorageGateway(blobServiceClientMock.Object, optionsMonitor, mockLogger.Object);

            using var stream = new MemoryStream();
            var result = sut.UploadAttachmentAsync(stream);

            // Assert
            result.Exception.Should().NotBeNull();
            result.Exception.Should().BeOfType<AggregateException>();
            result.Exception?.InnerException.Should().BeOfType<StatusCodeException>();
        }
    }
}
