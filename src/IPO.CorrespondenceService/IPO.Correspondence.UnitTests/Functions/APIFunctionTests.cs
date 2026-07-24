using AutoFixture;
using AwesomeAssertions;
using IPO.Common.Infrastructure;
using IPO.Correspondence.Interfaces.Services;
using IPO.Correspondence.Models;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using ApiFunctionNamespace = IPO.Correspondence.APIFunction;

namespace IPO.Correspondence.UnitTests.Functions
{
    [TestClass]
    public class APIFunctionTests
    {
        private readonly Fixture _fixture;
        private readonly Mock<ICorrespondenceManagementService> _mockCorrespondenceManagementService;
        private readonly Mock<ILogger> _mockLogger;

        public APIFunctionTests()
        {
            _fixture = new Fixture();
            _mockCorrespondenceManagementService = new Mock<ICorrespondenceManagementService>();
            _mockLogger = new Mock<ILogger>();
        }

        [TestMethod]
        public void APIRunReturnsOk()
        {
            //Arrange
            string payLoadData = "{ \"Email\":\"test@test.com\",\"TemplateId\":\"9bf26c69-429b-4351-818a-55195c46fc2f\", \"Personalisation\":{} }";
            var message = new NotificationMessage()
            {
                CreatedOn = DateTime.Now,
                PayLoad = payLoadData,
                OrganisationId = Guid.NewGuid(),

            };
            var messageJson = IPOJsonSerialization.Serialize(message);

            _mockCorrespondenceManagementService.Setup(s => s.ProcessNotificationMessageAsync(messageJson, NotificationType.API)).Returns(Task.CompletedTask);

            //Act
            var result = new ApiFunctionNamespace.APIFunction(_mockCorrespondenceManagementService.Object, _mockLogger.Object).RunAsync(messageJson);

            //Assert
            result.IsCanceled.Should().BeFalse();
            result.IsFaulted.Should().BeFalse();
            result.Exception.Should().BeNull();
            result.IsCompleted.Should().BeTrue();
            _mockLogger.Verify();
            _mockCorrespondenceManagementService.Verify();
        }

        [TestMethod]
        public void APIRunAsNullReturnsException()
        {
            //Arrange
            _mockCorrespondenceManagementService.Setup(s => s.ProcessNotificationMessageAsync(null!, NotificationType.API)).ThrowsAsync(new ArgumentNullException("value"));

            //Act
            var result = new ApiFunctionNamespace.APIFunction(_mockCorrespondenceManagementService.Object, _mockLogger.Object).RunAsync(null!);

            //Assert
            result.IsCanceled.Should().BeFalse();
            result.IsFaulted.Should().BeTrue();
            result.Exception.Should().NotBeNull();
            result.Exception!.InnerException!.Message.Should().Be("Value cannot be null. (Parameter 'value')");
            result.IsCompleted.Should().BeTrue();
            result.IsCompletedSuccessfully.Should().BeFalse();
            _mockCorrespondenceManagementService.Verify();
        }
    }
}