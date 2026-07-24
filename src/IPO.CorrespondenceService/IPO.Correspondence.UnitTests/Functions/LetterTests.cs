using AutoFixture;
using AwesomeAssertions;
using IPO.Common.Infrastructure;
using IPO.Correspondence.Interfaces.Services;
using IPO.Correspondence.Models;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using LetterFunctionNamespace = IPO.Correspondence.Letter;

namespace IPO.Correspondence.UnitTests.Functions
{
    [TestClass]
    public class LetterTests
    {
        private readonly Fixture _fixture;
        private readonly Mock<ICorrespondenceManagementService> _mockCorrespondenceManagementService;
        private readonly Mock<ILogger> _mockLogger;

        public LetterTests()
        {
            _fixture = new Fixture();
            _mockCorrespondenceManagementService = new Mock<ICorrespondenceManagementService>();
            _mockLogger = new Mock<ILogger>();
        }

        [TestMethod]
        public void LetterReturnsOk()
        {
            //Arrange
            string payLoadData = "{ \"Email\":\"test@test.com\",\"TemplateId\":\"9bf26c69-429b-4351-818a-55195c46fc2f\" }";
            var message = new NotificationMessage()
            {
                CreatedOn = DateTime.Now,
                Type = NotificationType.Letter,
                PayLoad = payLoadData,
                OrganisationId = Guid.NewGuid(),

            };
            var messageJson = IPOJsonSerialization.Serialize(message);

            _mockCorrespondenceManagementService.Setup(s => s.ProcessNotificationMessageAsync(messageJson, NotificationType.Letter)).Returns(Task.CompletedTask);

            //Act
            var result = new LetterFunctionNamespace.Letter(
                _mockCorrespondenceManagementService.Object,
                _mockLogger.Object)
                .Run(messageJson);

            //Assert
            result.IsCanceled.Should().BeFalse();
            result.IsFaulted.Should().BeFalse();
            result.Exception.Should().BeNull();
            result.IsCompleted.Should().BeTrue();
            _mockLogger.Verify();
            _mockCorrespondenceManagementService.Verify();
        }

        [TestMethod]
        public void LetterWithSpecifiedSendTypeReturnsOk()
        {
            //Arrange
            string payLoadData = "{ \"Email\":\"test@test.com\",\"TemplateId\":\"9bf26c69-429b-4351-818a-55195c46fc2f\" }";
            var message = new NotificationMessage()
            {
                CreatedOn = DateTime.Now,
                Type = NotificationType.Letter,
                PayLoad = payLoadData,
                APIKeyType = GovNotifyAPIKeyType.PretendToSend,
                OrganisationId = Guid.NewGuid(),

            };
            var messageJson = IPOJsonSerialization.Serialize(message);

            _mockCorrespondenceManagementService.Setup(s => s.ProcessNotificationMessageAsync(messageJson, NotificationType.Letter)).Returns(Task.CompletedTask);

            //Act
            var result = new LetterFunctionNamespace.Letter(
                _mockCorrespondenceManagementService.Object,
                _mockLogger.Object)
                .Run(messageJson);

            //Assert
            result.IsCanceled.Should().BeFalse();
            result.IsFaulted.Should().BeFalse();
            result.Exception.Should().BeNull();
            result.IsCompleted.Should().BeTrue();
            _mockLogger.Verify();
            _mockCorrespondenceManagementService.Verify();
        }

        [TestMethod]
        public void LetterAsNullReturnsException()
        {
            //Arrange
            _mockCorrespondenceManagementService.Setup(s => s.ProcessNotificationMessageAsync(null!, NotificationType.Letter)).ThrowsAsync(new ArgumentNullException("value"));

            //Act
            var result = new LetterFunctionNamespace.Letter(
                _mockCorrespondenceManagementService.Object,
                _mockLogger.Object)
                .Run(null!);

            //Assert
            result.IsCanceled.Should().BeFalse();
            result.IsFaulted.Should().BeTrue();
            result.Exception.Should().NotBeNull();
            result.Exception!.InnerException!.Message.Should().Be("Value cannot be null. (Parameter 'value')");
            result.IsCompleted.Should().BeTrue();
            result.IsCompletedSuccessfully.Should().BeFalse();
            _mockCorrespondenceManagementService.Verify();
        }

        [TestMethod]
        public void PrecompiledLetterReturnsOk()
        {
            //Arrange
            string payLoadData = "{ \"FileName\":\"test.pdf\",\"FileId\":\"00000000-0000-0000-0000-000000000000\",\"ClientReference\":\"10000000-0000-0000-0000-000000000000\",\"Postage\":\"FirstClass\"}";
            var message = new NotificationMessage()
            {
                OrganisationId = Guid.NewGuid(),
                CreatedOn = DateTime.Now,
                Type = NotificationType.PrecompiledLetter,
                PayLoad = payLoadData
            };
            var messageJson = IPOJsonSerialization.Serialize(message);

            _mockCorrespondenceManagementService.Setup(s => s.ProcessNotificationMessageAsync(messageJson, NotificationType.Letter)).Returns(Task.CompletedTask);

            //Act
            var result = new LetterFunctionNamespace.Letter(
                _mockCorrespondenceManagementService.Object,
                _mockLogger.Object)
                .Run(messageJson);

            //Assert
            result.IsCanceled.Should().BeFalse();
            result.IsFaulted.Should().BeFalse();
            result.Exception.Should().BeNull();
            result.IsCompleted.Should().BeTrue();
            _mockLogger.Verify();
            _mockCorrespondenceManagementService.Verify();
        }
    }
}