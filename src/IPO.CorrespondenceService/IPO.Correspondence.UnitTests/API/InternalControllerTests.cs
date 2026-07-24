using AutoFixture;
using AwesomeAssertions;
using IPO.Common.Infrastructure;
using IPO.Correspondence.API.Controllers;
using IPO.Correspondence.Interfaces.Services;
using IPO.Correspondence.Models;
using IPO.Correspondence.UnitTests.API;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System.Text.Json;

namespace IPO.FeeService.UnitTests.API
{
    [TestClass]
    public class InternalControllerTests
    {
        private readonly Mock<ICorrespondenceManagementService> _mockCorrespondenceManagementService;
        private readonly Fixture _fixture;

        public InternalControllerTests()
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

            _mockCorrespondenceManagementService.Setup(s => s.GetPendingNotificationsAsync(It.IsAny<Guid>(), true))
                                                .ReturnsAsync(expectedNotificationsViewModelSet)
                                                .Verifiable();

            var internalCorrespondenceApi = new InternalController(_mockCorrespondenceManagementService.Object);

            // Act
            var pendingNotificationsResult = await internalCorrespondenceApi.PendingNotifications(organisationId);
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

            _mockCorrespondenceManagementService.Setup(s => s.GetAllNotificationsFromTimeStampAsync(It.IsAny<Guid>(), It.IsAny<DateTime>(), It.IsAny<DateTime>(), true))
                                                .ReturnsAsync(expectedNotificationsViewModelSet)
                                                .Verifiable();

            var internalCorrespondenceApi = new InternalController(_mockCorrespondenceManagementService.Object);

            // Act
            var getNotificationsFromTimeStampResult = await internalCorrespondenceApi.GetAllNotificationsFromTimeStamp(organisationId, from, to);
            var getNotificationsFromTimeStampResults = (OkObjectResult)getNotificationsFromTimeStampResult.Result!;
            var results = (IEnumerable<NotificationViewModel>)getNotificationsFromTimeStampResults.Value!;

            // Assert  
            results.Should().NotBeNull();
            results.Should().ContainInOrder(expectedNotificationsViewModelSet);
            _mockCorrespondenceManagementService.Verify();
        }

        [TestMethod]
        public async Task PostNotificationAsyncReturnsAccepted()
        {
            // Arrange
            var organisationId = Guid.NewGuid();
            var notificationModel = _fixture.Create<NotificationModel>();
            var acceptedId = Guid.NewGuid();

            _mockCorrespondenceManagementService.Setup(s => s.PostNotificationAsync(It.IsAny<Guid>(), It.IsAny<NotificationModel>()))
                                                .ReturnsAsync(acceptedId)
                                                .Verifiable();

            var internalCorrespondenceApi = new InternalController(_mockCorrespondenceManagementService.Object);

            // Act
            var postNotificationResult = await internalCorrespondenceApi.PostNotification(organisationId, notificationModel);
            var result = postNotificationResult;

            // Assert  
            result.Should().NotBeNull();
            _mockCorrespondenceManagementService.Verify();
        }

        [TestMethod]
        public async Task PostEmailWithFileNotificationAsyncReturnsAccepted()
        {
            // Arrange
            var organisationId = Guid.NewGuid();
            var content = _fixture.Create<Dictionary<string, string>>()
                .ToDictionary(kvp => kvp.Key, kvp => (object)kvp.Value);
            var mockFile = HttpContextFactory.CreateMockFormFile(IPOJsonSerialization.Serialize(content), "test.txt");
            var httpContext = HttpContextFactory.CreateHttpContext(content, new IFormFile[] { mockFile.Object });

            var acceptedId = Guid.NewGuid();

            _mockCorrespondenceManagementService.Setup(s => s.PostEmailWithFileNotificationAsync(It.IsAny<PostEmailWithFileNotificationRequestModel>()))
                                                .ReturnsAsync(acceptedId)
                                                .Verifiable();

            var internalCorrespondenceApi = new InternalController(_mockCorrespondenceManagementService.Object)
            {
                ControllerContext = new ControllerContext()
                {
                    HttpContext = httpContext
                }
            };

            // Act
            var postNotificationResult = await internalCorrespondenceApi.PostEmailWithFileNotification(
                new PostEmailWithFileNotificationRequestModel()
                {
                    organisationId = organisationId,
                    Content = content,
                    File = mockFile.Object
                });
            var result = postNotificationResult;

            // Assert  
            result.Should().NotBeNull();
            _mockCorrespondenceManagementService.Verify();
        }

        [TestMethod]
        public async Task PostEmailWithFileNotificationAsyncReturnsUnproccesibleEntityForInvalidJsonContent()
        {
            // Arrange
            var organisationId = Guid.NewGuid();
            var content = "{{}}}";
            var mockFile = HttpContextFactory.CreateMockFormFile(IPOJsonSerialization.Serialize(content), "test.txt");
            var httpContext = HttpContextFactory.CreateHttpContext(content, new IFormFile[] { mockFile.Object });
            var error = new Error()
            {
                Code = "422",
                Description = "Invalid Json"
            };

            _mockCorrespondenceManagementService.Setup(s => s.PostEmailWithFileNotificationAsync(It.IsAny<PostEmailWithFileNotificationRequestModel>()))
                                                .ThrowsAsync(new StatusCodeException(error, "Invalid Json", null, 422))
                                                .Verifiable();

            var internalCorrespondenceApi = new InternalController(_mockCorrespondenceManagementService.Object)
            {
                ControllerContext = new ControllerContext()
                {
                    HttpContext = httpContext
                }
            };

            //Act & Assert  
            await Assert.ThrowsExceptionAsync<StatusCodeException>(() =>
                internalCorrespondenceApi.PostEmailWithFileNotification(
                    new PostEmailWithFileNotificationRequestModel()
                    {
                        organisationId = organisationId,
                        Content = It.IsAny<Dictionary<string, object>>(),
                        File = mockFile.Object
                    }));
        }

        [TestMethod]
        public async Task PostPrecompiledLetterRequestAsyncReturnsAccepted()
        {
            // Arrange
            var organisationId = Guid.NewGuid();
            var content = _fixture.Create<Dictionary<string, string>>()
                .ToDictionary(kvp => kvp.Key, kvp => (object)kvp.Value);
            var mockFile = HttpContextFactory.CreateMockFormFile(IPOJsonSerialization.Serialize(content), "test.pdf");
            var httpContext = HttpContextFactory.CreateHttpContext(content, new IFormFile[] { mockFile.Object });

            var acceptedId = Guid.NewGuid();

            _mockCorrespondenceManagementService.Setup(s => s.PostPrecompiledLetterRequestAsync(It.IsAny<PrecompiledLetterRequestModel>()))
                                                .ReturnsAsync(acceptedId)
                                                .Verifiable();

            var internalCorrespondenceApi = new InternalController(_mockCorrespondenceManagementService.Object)
            {
                ControllerContext = new ControllerContext()
                {
                    HttpContext = httpContext
                }
            };

            // Act
            var postPrecompiledLetterRequestResult = await internalCorrespondenceApi.PostPrecompiledLetterRequest(
                new PrecompiledLetterRequestModel()
                {
                    organisationId = organisationId,
                    File = mockFile.Object,
                    Postage = PrecompiledLetterPostage.FirstClass
                });
            var result = postPrecompiledLetterRequestResult;

            // Assert  
            result.Should().NotBeNull();
            _mockCorrespondenceManagementService.Verify();
        }

        [TestMethod]
        public async Task GenerateTemplatePreviewAsyncReturnsAccepted()
        {
            // Arrange

            var preview = _fixture.Create<TemplatePreviewRequest>();
            var response = _fixture.Create<TemplatePreviewResponse>();

            _mockCorrespondenceManagementService.Setup(s => s.GenerateTemplatePreviewAsync(It.IsAny<TemplatePreviewRequest>()))
                                                .ReturnsAsync(response)
                                                .Verifiable();

            var internalCorrespondenceApi = new InternalController(_mockCorrespondenceManagementService.Object);

            // Act
            var postNotificationResult = await internalCorrespondenceApi.PostPreviewTemplate(preview);
            var result = postNotificationResult;

            // Assert  
            result.Should().NotBeNull();
            _mockCorrespondenceManagementService.Verify();
        }
    }
}