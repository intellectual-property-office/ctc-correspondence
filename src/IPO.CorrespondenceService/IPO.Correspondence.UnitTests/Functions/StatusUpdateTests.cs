using AutoFixture;
using AwesomeAssertions;
using IPO.Correspondence.Interfaces.Services;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using StatusUpdatefunctionNamespace = IPO.Correspondence.StatusUpdate;
using Microsoft.Extensions.Logging;
using Microsoft.Azure.Functions.Worker;

namespace IPO.Correspondence.UnitTests.Functions
{
    [TestClass]
    public class StatusUpdateTests
    {
        private readonly Fixture _fixture;
        private readonly Mock<ICorrespondenceManagementService> _mockCorrespondenceManagementService;
        private readonly Mock<ILogger> _mockLogger;

        public StatusUpdateTests()
        {
            _fixture = new Fixture();
            _mockCorrespondenceManagementService = new Mock<ICorrespondenceManagementService>();
            _mockLogger = new Mock<ILogger>();
        }

        [TestMethod]
        public void StatusUpdateReturnsOk()
        {
            //Arrange
            _mockCorrespondenceManagementService.Setup(s => s.UpdateNotificationStatusesAsync()).Returns(Task.CompletedTask);

            //Act
            var result = new StatusUpdatefunctionNamespace.StatusUpdate(_mockCorrespondenceManagementService.Object, _mockLogger.Object).Run(default(TimerInfo)!);

            //Assert
            result.IsCanceled.Should().BeFalse();
            result.IsFaulted.Should().BeFalse();
            result.Exception.Should().BeNull();
            result.IsCompleted.Should().BeTrue();
            _mockLogger.Verify();
            _mockCorrespondenceManagementService.Verify();
        }

        [TestMethod]
        public void StatusUpdateAsNullReturnsException()
        {
            //Arrange
            _mockCorrespondenceManagementService.Setup(s => s.UpdateNotificationStatusesAsync()).Returns(Task.CompletedTask);

            //Act
            var result = new StatusUpdatefunctionNamespace.StatusUpdate(_mockCorrespondenceManagementService.Object, null!).Run(null!);

            //Assert
            result.IsCanceled.Should().BeFalse();
            result.IsFaulted.Should().BeTrue();
            result.Exception.Should().NotBeNull();
            result.Exception!.InnerException!.Message.Should().Be("Value cannot be null. (Parameter 'logger')");
            result.IsCompleted.Should().BeTrue();
            result.IsCompletedSuccessfully.Should().BeFalse();
            _mockCorrespondenceManagementService.Verify();
        }
    }
}