using AutoFixture;
using AwesomeAssertions;
using IPO.Correspondence.API.Controllers;
using IPO.Correspondence.Interfaces.Services;
using IPO.Correspondence.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace IPO.Correspondence.UnitTests.API
{
    [TestClass]
    public class ExternalControllerTests
    {
        private readonly Mock<ICorrespondenceManagementService> _mockCorrespondenceManagementService;
        private readonly Fixture _fixture;

        public ExternalControllerTests()
        {
            _mockCorrespondenceManagementService = new Mock<ICorrespondenceManagementService>();
            _fixture = new Fixture();
        }

        [TestMethod]
        public async Task PendingNotificationsAsyncReturnsOk()
        {
            // Arrange
            var organisationId = Guid.NewGuid();
            var expectedNotificationsViewModelSet = _fixture.Create<IEnumerable<NotificationViewModel>>();

            _mockCorrespondenceManagementService.Setup(s => s.GetPendingNotificationsAsync(It.IsAny<Guid>(), false))
                                         .ReturnsAsync(expectedNotificationsViewModelSet)
                                         .Verifiable();

            var externalCorrespondenceApi = new ExternalController(_mockCorrespondenceManagementService.Object);

            // Act
            var pendingNotificationsResult = await externalCorrespondenceApi.PendingNotifications(organisationId);
            var pendingNotificationsResults = (OkObjectResult)pendingNotificationsResult.Result!;
            var results = (IEnumerable<NotificationViewModel>)pendingNotificationsResults.Value!;

            // Assert  
            results.Should().NotBeNull();
            results.Should().ContainInOrder(expectedNotificationsViewModelSet);
            _mockCorrespondenceManagementService.Verify();
        }

        [TestMethod]
        public async Task GetAllNotificationsFromTimeStampAsyncReturnsOk()
        {
            // Arrange
            var organisationId = Guid.NewGuid();
            var from = DateTime.Now.AddMinutes(-10);
            var to = DateTime.Now;
            var expectedNotificationsViewModelSet = _fixture.Create<IEnumerable<NotificationViewModel>>();

            _mockCorrespondenceManagementService.Setup(s => s.GetAllNotificationsFromTimeStampAsync(It.IsAny<Guid>(), It.IsAny<DateTime>(), It.IsAny<DateTime>(), false))
                                         .ReturnsAsync(expectedNotificationsViewModelSet)
                                         .Verifiable();

            var externalCorrespondenceApi = new ExternalController(_mockCorrespondenceManagementService.Object);

            // Act
            var pendingNotificationsResult = await externalCorrespondenceApi.GetAllNotificationsFromTimeStamp(organisationId, from, to);
            var pendingNotificationsResults = (OkObjectResult)pendingNotificationsResult.Result!;
            var results = (IEnumerable<NotificationViewModel>)pendingNotificationsResults.Value!;

            // Assert  
            results.Should().NotBeNull();
            results.Should().ContainInOrder(expectedNotificationsViewModelSet);
            _mockCorrespondenceManagementService.Verify();
        }

        [TestMethod]
        public async Task PeekNotificationsAsyncReturnsOk()
        {
            // Arrange
            var correspondenceId = Guid.NewGuid();
            var expectedNotificationsViewModel = _fixture.Create<NotificationViewModel>();

            _mockCorrespondenceManagementService.Setup(s => s.PeekNotificationAsync(It.IsAny<Guid>()))
                                         .ReturnsAsync(expectedNotificationsViewModel)
                                         .Verifiable();

            var externalCorrespondenceApi = new ExternalController(_mockCorrespondenceManagementService.Object);

            // Act
            var pendingNotificationsResult = await externalCorrespondenceApi.PeekNotification(correspondenceId);
            var pendingNotificationsResults = (OkObjectResult)pendingNotificationsResult.Result!;
            var results = (NotificationViewModel)pendingNotificationsResults.Value!;

            // Assert  
            results.Should().NotBeNull();
            results.Should().Be(expectedNotificationsViewModel);
            _mockCorrespondenceManagementService.Verify();
        }
    }
}
