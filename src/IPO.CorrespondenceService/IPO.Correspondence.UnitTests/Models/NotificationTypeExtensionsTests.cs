using AwesomeAssertions;
using IPO.Correspondence.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace IPO.Correspondence.UnitTests.Models
{
    [TestClass]
    public class NotificationTypeExtensionsTests
    {
        [DataTestMethod]
        [DataRow(NotificationType.API, "API")]
        [DataRow(NotificationType.Email, "Email")]
        [DataRow(NotificationType.Letter, "Letter")]
        [DataRow(NotificationType.SMS, "SMS")]
        public void ToMessageTypeStringConversionTest(NotificationType type, string expectedResult)
        {
            // Arrange

            // Act
            var res = type.ToMessageType();

            // Assert
            res.Should().NotBeNull();
            res.Value.Should().Be(expectedResult);  
        }
    }
}
