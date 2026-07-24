using Azure.Messaging.ServiceBus;
using AwesomeAssertions;
using IPO.Correspondence.Interfaces.Gateways;
using IPO.Correspondence.Messaging;
using IPO.Correspondence.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace IPO.Correspondence.UnitTests.Messaging
{
    [TestClass]
    public class MessagingClientTests
    {
        [TestMethod]
        public void SendNotificationMessageAsyncSuccessfully()
        {
            // Arrange
            var notificationMessage = new NotificationMessage
            {

            };

            var serviceBusSender = new Mock<ServiceBusSender>(MockBehavior.Strict);

            serviceBusSender.Setup(s => s.SendMessageAsync(
                It.IsAny<ServiceBusMessage>(), 
                It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask)
                .Verifiable();

            var correspondenceSurviceValidator = new Mock<ICorrespondenceServiceValidator>(MockBehavior.Strict);

            var sut = new MessagingClient(serviceBusSender.Object, correspondenceSurviceValidator.Object);

            // Act
            var res = sut.SendNotificationMessageAsync(notificationMessage);

            // Assert
            res.Status.Should().Be(TaskStatus.RanToCompletion);

        }
    }
}
