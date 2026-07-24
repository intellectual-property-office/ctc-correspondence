using AutoFixture;
using AwesomeAssertions;
using IPO.Correspondence.Interfaces.Services;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using EmailFunctionNamespace = IPO.Correspondence.Email;
using IPO.Correspondence.Models;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using IPO.Common.Infrastructure;

namespace IPO.Correspondence.UnitTests.Functions
{
    [TestClass]
    public class EmailTests
    {
        private readonly Fixture _fixture;
        private readonly Mock<ICorrespondenceManagementService> _mockCorrespondenceManagementService;
        private readonly Mock<ILogger> _mockLogger;
        public EmailTests()
        {
            _fixture = new Fixture();
            _mockCorrespondenceManagementService = new Mock<ICorrespondenceManagementService>();
            _mockLogger = new Mock<ILogger>();
        }

        [TestMethod]
        public void EmailRunWithNoFileIDReturnsOk()
        {
            //Arrange
            string payLoadData = "{ \"Email\":\"test@test.com\",\"TemplateId\":\"9bf26c69-429b-4351-818a-55195c46fc2f\"}";
            var message = new NotificationMessage()
            {
                CreatedOn = DateTime.Now,
                PayLoad = payLoadData,
                OrganisationId = Guid.NewGuid()
            };
            var messageJson = IPOJsonSerialization.Serialize(message);

            _mockCorrespondenceManagementService.Setup(s => s.ProcessNotificationMessageAsync(messageJson, NotificationType.Email)).Returns(Task.CompletedTask);

            //Act
            var result = new EmailFunctionNamespace.Email(
                _mockCorrespondenceManagementService.Object,
                _mockLogger.Object)
                .Run(messageJson);

            //Assert
            result.IsCanceled.Should().BeFalse();
            result.IsFaulted.Should().BeFalse();
            result.Exception.Should().BeNull();
            result.IsCompleted.Should().BeTrue();
            _mockCorrespondenceManagementService.Verify();
        }

        [TestMethod]
        public void EmailRunWithFileIDReturnsOk()
        {
            //Arrange
            string payLoadData = "{ \"Email\":\"test@test.com\",\"TemplateId\":\"9bf26c69-429b-4351-818a-55195c46fc2f\",\"FileId\":\"9bf26c69-429b-4351-818a-55195c46fc2f\"}";

            var message = new NotificationMessage()
            {
                CreatedOn = DateTime.Now,
                PayLoad = payLoadData,
                OrganisationId = Guid.NewGuid()
            };
            var messageJson = IPOJsonSerialization.Serialize(message);

            _mockCorrespondenceManagementService.Setup(s => s.ProcessNotificationMessageAsync(messageJson, NotificationType.Email)).Returns(Task.CompletedTask);

            //Act
            var result = new EmailFunctionNamespace.Email(
                _mockCorrespondenceManagementService.Object,
                _mockLogger.Object)
                .Run(messageJson);

            //Assert
            result.IsCanceled.Should().BeFalse();
            result.IsFaulted.Should().BeFalse();
            result.Exception.Should().BeNull();
            result.IsCompleted.Should().BeTrue();
            _mockCorrespondenceManagementService.Verify();
        }

        [TestMethod]
        public void EmailRunWithReplyToIDReturnsOk()
        {
            //Arrange
            string payLoadData = "{ \"Email\":\"test@test.com\",\"TemplateId\":\"9bf26c69-429b-4351-818a-55195c46fc2f\",\"EmailReplyToId\":\"00d6ed8c-986e-45e3-acc3-33ad8cf499a6\"}";

            var message = new NotificationMessage()
            {
                CreatedOn = DateTime.Now,
                PayLoad = payLoadData,
                OrganisationId = Guid.NewGuid()
            };
            var messageJson = IPOJsonSerialization.Serialize(message);

            _mockCorrespondenceManagementService.Setup(s => s.ProcessNotificationMessageAsync(messageJson, NotificationType.Email)).Returns(Task.CompletedTask);

            //Act
            var result = new EmailFunctionNamespace.Email(
                _mockCorrespondenceManagementService.Object,
                _mockLogger.Object)
                .Run(messageJson);

            //Assert
            result.IsCanceled.Should().BeFalse();
            result.IsFaulted.Should().BeFalse();
            result.Exception.Should().BeNull();
            result.IsCompleted.Should().BeTrue();
            _mockCorrespondenceManagementService.Verify();
        }

        [TestMethod]
        public void EmailRunWithSpecifiedSendTypeReturnsOk()
        {
            //Arrange
            string payLoadData = "{ \"Email\":\"test@test.com\",\"TemplateId\":\"9bf26c69-429b-4351-818a-55195c46fc2f\"}";

            var message = new NotificationMessage()
            {
                CreatedOn = DateTime.Now,
                PayLoad = payLoadData,
                APIKeyType = GovNotifyAPIKeyType.PretendToSend,
                OrganisationId = Guid.NewGuid()
            };
            var messageJson = IPOJsonSerialization.Serialize(message);

            //Act
            _mockCorrespondenceManagementService.Setup(s => s.ProcessNotificationMessageAsync(messageJson, NotificationType.Email)).Returns(Task.CompletedTask);

            var result = new EmailFunctionNamespace.Email(
                _mockCorrespondenceManagementService.Object,
                _mockLogger.Object)
                .Run(messageJson);

            //Assert
            result.IsCanceled.Should().BeFalse();
            result.IsFaulted.Should().BeFalse();
            result.Exception.Should().BeNull();
            result.IsCompleted.Should().BeTrue();
            _mockCorrespondenceManagementService.Verify();
        }

        [TestMethod]
        public void EmailRunAsNullReturnsException()
        {
            //Arrange
            _mockCorrespondenceManagementService.Setup(s => s.ProcessNotificationMessageAsync(null!, NotificationType.Email)).ThrowsAsync(new ArgumentNullException("value"));

            //Act
            var result = new EmailFunctionNamespace.Email(
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
    }
}