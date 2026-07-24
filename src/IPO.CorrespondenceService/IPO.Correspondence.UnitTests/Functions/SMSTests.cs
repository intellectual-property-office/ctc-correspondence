using AutoFixture;
using AwesomeAssertions;
using IPO.Common.Infrastructure;
using IPO.Correspondence.Interfaces.Services;
using IPO.Correspondence.Models;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using SmsFunctionNamespace = IPO.Correspondence.SMS;

namespace IPO.Correspondence.UnitTests.Functions
{
    [TestClass]
    public class SMSTests
    {
        private readonly Fixture _fixture;
        private readonly Mock<ICorrespondenceManagementService> _mockCorrespondenceManagementService;
        private readonly Mock<ILogger> _mockLogger;

        public SMSTests()
        {
            _fixture = new Fixture();
            _mockCorrespondenceManagementService = new Mock<ICorrespondenceManagementService>();
            _mockLogger = new Mock<ILogger>();
        }

        [TestMethod]
        public void SmsReturnsOk()
        {
            //Arrange
            string payLoadData = "{ \"Email\":\"test@test.com\",\"TemplateId\":\"9bf26c69-429b-4351-818a-55195c46fc2f\" }";

            var message = new NotificationMessage()
            {
                CreatedOn = DateTime.Now,
                PayLoad = payLoadData,
                OrganisationId = Guid.NewGuid(),

            };
            var messageJson = IPOJsonSerialization.Serialize(message);

            _mockCorrespondenceManagementService.Setup(s => s.ProcessNotificationMessageAsync(messageJson, NotificationType.SMS)).Returns(Task.CompletedTask);

            //Act
            var result = new SmsFunctionNamespace.Sms(
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
        public void SmsWithSpecifiedSendTypeReturnsOk()
        {
            //Arrange
            string payLoadData = "{ \"Email\":\"test@test.com\",\"TemplateId\":\"9bf26c69-429b-4351-818a-55195c46fc2f\" }";

            var message = new NotificationMessage()
            {
                CreatedOn = DateTime.Now,
                PayLoad = payLoadData,
                APIKeyType = GovNotifyAPIKeyType.PretendToSend,
                OrganisationId = Guid.NewGuid(),

            };
            var messageJson = IPOJsonSerialization.Serialize(message);

            _mockCorrespondenceManagementService.Setup(s => s.ProcessNotificationMessageAsync(messageJson, NotificationType.SMS)).Returns(Task.CompletedTask);

            //Act
            var result = new SmsFunctionNamespace.Sms(
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
        public void SmsAsNullReturnsException()
        {
            //Arrange
            _mockCorrespondenceManagementService.Setup(s => s.ProcessNotificationMessageAsync(null!, NotificationType.SMS)).ThrowsAsync(new ArgumentNullException("value"));

            //Act
            var result = new SmsFunctionNamespace.Sms(
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