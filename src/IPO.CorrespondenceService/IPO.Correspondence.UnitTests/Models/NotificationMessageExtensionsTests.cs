using AwesomeAssertions;
using IPO.Common.Infrastructure;
using IPO.Correspondence.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace IPO.Correspondence.UnitTests.Models
{
    [TestClass]
    public class NotificationMessageExtensionsTests
    {
        [TestMethod]
        public void ToEmailAttachmentIdReturnsFileId()
        {
            // Arrange
            var fileId = Guid.NewGuid().ToString();
            var payload = IPOJsonSerialization.Serialize(new EmailContent() { FileId = fileId });
            var notificationMessage = new NotificationMessage() { PayLoad = payload };

            // Act
            var result = NotificationMessageExtensions.ToEmailAttachmentId(notificationMessage);

            // Assert
            result.Should().NotBeNull();
            result.Should().Be(fileId);
        }

        [TestMethod]
        public void ToEmailAttachmentIdWhenDeserializeFailsReturnsNull()
        {
            // Arrange
            var notificationMessage = new NotificationMessage() { PayLoad = "Bad message" };

            // Act
            var result = NotificationMessageExtensions.ToEmailAttachmentId(notificationMessage);

            // Assert
            result.Should().BeNull();
        }
    }
}
