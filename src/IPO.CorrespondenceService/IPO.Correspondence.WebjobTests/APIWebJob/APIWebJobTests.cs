using AutoFixture;
using FluentAssertions;
using IPO.Correspondence.APIWebJob;
using IPO.Correspondence.Interfaces.Notifications;
using IPO.Correspondence.Models;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using FuncWebJob = IPO.Correspondence.APIWebJob;

namespace IPO.Correspondence.UnitTests.APIWebJob
{
    [TestClass]
    public class APIWebJobTests
    {
        private readonly Fixture _fixture;
        private readonly Mock<INotificationDbRepository> _mockNotificationDbRepository;
        private readonly Mock<ILogger<FuncWebJob.Function>> _mockLogger;
        private readonly Mock<IMessagingClient> _mockClient;

        public APIWebJobTests()
        {
            _mockNotificationDbRepository = new Mock<INotificationDbRepository>();
            _mockLogger = new Mock<ILogger<FuncWebJob.Function>>();
            _fixture = new Fixture();
            _mockClient = new Mock<IMessagingClient>();
        }

        [TestMethod]
        public void APIWebJobReturnsOk()
        {
            // Arrange
            var funcWebJob = new Function(_mockClient.Object, _mockNotificationDbRepository.Object, _mockLogger.Object);
            var notificationMessage = _fixture.Create<NotificationMessage>();
            _mockNotificationDbRepository.Setup(r => r.NotifyOwnersForPendingNotificationsAsync(It.IsAny<Func<IEnumerable<NotificationOwner>, Task>>())).Returns(Task.CompletedTask);

            // Act
            var result = funcWebJob.RunAsync();

            // Assert
            result.IsCanceled.Should().BeFalse();
            result.IsFaulted.Should().BeFalse();
            result.IsCompleted.Should().BeTrue();
            result.IsCompletedSuccessfully.Should().BeTrue();
            _mockNotificationDbRepository.Verify();
        }

        [TestMethod]
        public void APIWebJobHelperCreateNotificationMessageReturnsOK()
        {
            // Arrange
            var templateId = Guid.NewGuid().ToString();
            var notificationowner = new NotificationOwner
            {
                Id = Guid.NewGuid(),
                Email = "test@gmail.com",
                TemplateId = templateId
            };

            // Act
            var webjobObj = new APIWebJobHelperTests(_mockClient.Object, _mockNotificationDbRepository.Object, _mockLogger.Object);
            var result = webjobObj.TestCreateNotificationMessage(notificationowner);

            // Assert
            result.OrganisationId.Should().Be(notificationowner.Id);
            result.PayLoad.Should().NotBeNullOrEmpty();
            result.Status.Should().Be(NotificationStatus.SendingToNotify);
            result.Type.Should().Be(NotificationType.Email);
        }

        [TestMethod]
        public void APIWebJobHelperSendNotificationToOwnersAsyncReturnsOK()
        {
            // Arrange
            var owners = _fixture.Create<IEnumerable<NotificationOwner>>();
            _mockClient.Setup(r => r.SendNotificationMessageAsync(It.IsAny<NotificationMessage>())).Returns(Task.CompletedTask);

            // Act
            var webjobObj = new APIWebJobHelperTests(_mockClient.Object, _mockNotificationDbRepository.Object, _mockLogger.Object);
            var result = webjobObj.TestSendNotificationToOwnersAsync(owners);

            // Assert
            result.IsCanceled.Should().BeFalse();
            result.IsFaulted.Should().BeFalse();
            result.Exception.Should().BeNull();
            result.IsCompleted.Should().BeTrue();
            _mockClient.Verify();
        }

    }
}
