using AutoFixture;
using AwesomeAssertions;
using IPO.Common.Infrastructure;
using IPO.Correspondence.Interfaces;
using IPO.Correspondence.Interfaces.Gateways;
using IPO.Correspondence.Interfaces.Notifications;
using IPO.Correspondence.Models;
using IPO.Correspondence.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System.Text;

namespace IPO.Correspondence.UnitTests.Services
{
    [TestClass]
    public class CorrespondenceManagementServiceTests
    {
        private Fixture? _fixture;
        private Mock<INotificationDbRepository>? _repository;
        private Mock<IMessagingClient>? _messagingClient;
        private Mock<IFileStorageGateway>? _fileStorage;
        private Mock<IPreliminaryFileValidator>? _fileValidator;
        private Mock<INotifyGateway>? _notifyGateway;
        private Mock<ILogger>? _logger;
        private CorrespondenceManagementService? _sut;

        [TestInitialize]
        public void Setup()
        {
            _fixture = new Fixture();
            _repository = new Mock<INotificationDbRepository>();
            _messagingClient = new Mock<IMessagingClient>();
            _fileStorage = new Mock<IFileStorageGateway>();
            _fileValidator = new Mock<IPreliminaryFileValidator>();
            _notifyGateway = new Mock<INotifyGateway>();
            _logger = new Mock<ILogger>();

            _sut = new CorrespondenceManagementService(
                _repository.Object,
                _messagingClient.Object,
                _fileStorage.Object,
                _fileValidator.Object,
                _notifyGateway.Object,
                _logger.Object);
        }

        #region Helper Methods

        private static NotificationMessage CreateNotificationMessage(NotificationType type, object payload)
        {
            return new NotificationMessage
            {
                Id = Guid.NewGuid(),
                OrganisationId = Guid.NewGuid(),
                APIKeyType = GovNotifyAPIKeyType.PretendToSend,
                Type = type,
                PayLoad = IPOJsonSerialization.Serialize(payload),
                CreatedOn = DateTime.UtcNow
            };
        }

        private void SetupRepositoryStore(string externalId = null!) =>
            _repository!.Setup(r => r.StoreAsync(It.IsAny<NotificationMessage>(), externalId))
                .Returns(Task.CompletedTask);

        private static FormFile CreateFormFile(string content, string fileName)
        {
            var bytes = Encoding.UTF8.GetBytes(content);
            var stream = new MemoryStream(bytes);
            return new FormFile(stream, 0, bytes.Length, "file", fileName);
        }

        #endregion

        #region Repository Proxy Tests

        [TestMethod]
        public async Task GetPendingNotificationsAsync_ProxiesToRepository_WithIsInternalFalse()
        {
            // Arrange
            var ownerId = _fixture.Create<Guid>();
            var expected = _fixture.CreateMany<NotificationViewModel>(3);

            _repository!.Setup(r => r.GetPendingNotificationsAsync(ownerId, false))
                .ReturnsAsync(expected);

            // Act
            var result = await _sut!.GetPendingNotificationsAsync(ownerId, false);

            // Assert
            result.Should().BeEquivalentTo(expected);
            _repository.Verify(r => r.GetPendingNotificationsAsync(ownerId, false), Times.Once);
        }

        [TestMethod]
        public async Task GetAllNotificationsFromTimeStampAsync_ProxiesToRepository_WithIsInternalFalse()
        {
            // Arrange
            var ownerId = _fixture.Create<Guid>();
            var from = DateTime.UtcNow.AddHours(-1);
            var to = DateTime.UtcNow;
            var expected = _fixture.CreateMany<NotificationViewModel>(3);

            _repository!.Setup(r => r.GetAllNotificationsFromTimeStampAsync(ownerId, from, to, false))
                .ReturnsAsync(expected);

            // Act
            var result = await _sut!.GetAllNotificationsFromTimeStampAsync(ownerId, from, to, false);

            // Assert
            result.Should().BeEquivalentTo(expected);
            _repository.Verify(r => r.GetAllNotificationsFromTimeStampAsync(ownerId, from, to, false), Times.Once);
        }

        [TestMethod]
        public async Task PeekNotificationAsync_ProxiesToRepository()
        {
            // Arrange
            var id = _fixture.Create<Guid>();
            var expected = _fixture.Create<NotificationViewModel>();

            _repository!.Setup(r => r.PeekNotificationAsync(id))
                .ReturnsAsync(expected);

            // Act
            var result = await _sut!.PeekNotificationAsync(id);

            // Assert
            result.Should().Be(expected);
            _repository.Verify(r => r.PeekNotificationAsync(id), Times.Once);
        }

        #endregion

        #region Messaging Client Tests

        [TestMethod]
        public async Task SendNotificationMessageAsync_CallsMessagingClient_AndReturnsId()
        {
            // Arrange
            var message = _fixture!.Build<NotificationMessage>()
                .With(m => m.Id, Guid.NewGuid())
                .Create();

            _messagingClient!.Setup(m => m.SendNotificationMessageAsync(message))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _sut!.SendNotificationMessageAsync(message);

            // Assert
            result.Should().Be(message.Id);
            _messagingClient.Verify(m => m.SendNotificationMessageAsync(message), Times.Once);
        }

        [TestMethod]
        public async Task PostNotificationAsync_BuildsMessage_SendsToMessagingClient_AndReturnsGeneratedId()
        {
            // Arrange
            var organisationId = _fixture.Create<Guid>();
            var content = new Dictionary<string, object?>
            {
                ["TemplateId"] = Guid.NewGuid().ToString(),
                ["EmailAddress"] = "test@test.com",
                ["Personalisation"] = new Dictionary<string, object>()
            };
            var model = new NotificationModel
            {
                Channel = NotificationType.Email,
                SendType = GovNotifyAPIKeyType.PretendToSend,
                Content = content!
            };

            _messagingClient!.Setup(m => m.SendNotificationMessageAsync(It.IsAny<NotificationMessage>()))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _sut!.PostNotificationAsync(organisationId, model);

            // Assert
            result.Should().NotBe(Guid.Empty);
            _messagingClient.Verify(m => m.SendNotificationMessageAsync(
                It.Is<NotificationMessage>(n =>
                    n.OrganisationId == organisationId &&
                    n.Type == NotificationType.Email &&
                    n.APIKeyType == GovNotifyAPIKeyType.PretendToSend &&
                    n.PayLoad == IPOJsonSerialization.Serialize(content))),
                Times.Once);
        }

        #endregion

        #region Email With File Tests

        [TestMethod]
        public async Task PostEmailWithFileNotificationAsync_ValidRequest_ReturnsGuid()
        {
            // Arrange
            var organisationId = _fixture!.Create<Guid>();
            var file = CreateFormFile("test content", "test.pdf");
            var content = new Dictionary<string, object>
            {
                ["TemplateId"] = Guid.NewGuid().ToString(),
                ["EmailAddress"] = "test@test.com",
                ["Personalisation"] = new Dictionary<string, object> { ["name"] = "Test" }
            };

            var model = new PostEmailWithFileNotificationRequestModel
            {
                organisationId = organisationId,
                SendType = GovNotifyAPIKeyType.PretendToSend,
                Content = content,
                File = file
            };

            var fileId = "blob-file-id-123";
            _fileValidator!.Setup(v => v.Validate(file));
            _fileStorage!.Setup(fs => fs.UploadAttachmentAsync(It.IsAny<Stream>())).ReturnsAsync(fileId);
            _messagingClient!.Setup(m => m.SendNotificationMessageAsync(It.IsAny<NotificationMessage>()))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _sut!.PostEmailWithFileNotificationAsync(model);

            // Assert
            result.Should().NotBe(Guid.Empty);
            _fileValidator.Verify(v => v.Validate(file), Times.Once);
            _fileStorage.Verify(fs => fs.UploadAttachmentAsync(It.IsAny<Stream>()), Times.Once);
            _messagingClient.Verify(m => m.SendNotificationMessageAsync(
                It.Is<NotificationMessage>(n =>
                    n.OrganisationId == organisationId &&
                    n.Type == NotificationType.Email &&
                    n.APIKeyType == GovNotifyAPIKeyType.PretendToSend &&
                    n.Status == NotificationStatus.SendingToNotify)), Times.Once);
        }

        [TestMethod]
        public async Task PostEmailWithFileNotificationAsync_AddsFileNameAndFileIdToContent()
        {
            // Arrange
            var organisationId = _fixture!.Create<Guid>();
            var fileName = "document.pdf";
            var file = CreateFormFile("test content", fileName);
            var content = new Dictionary<string, object>
            {
                ["TemplateId"] = Guid.NewGuid().ToString(),
                ["EmailAddress"] = "test@test.com"
            };

            var model = new PostEmailWithFileNotificationRequestModel
            {
                organisationId = organisationId,
                SendType = GovNotifyAPIKeyType.PretendToSend,
                Content = content,
                File = file
            };

            var fileId = "blob-file-id-456";
            _fileValidator!.Setup(v => v.Validate(file));
            _fileStorage!.Setup(fs => fs.UploadAttachmentAsync(It.IsAny<Stream>())).ReturnsAsync(fileId);
            _messagingClient!.Setup(m => m.SendNotificationMessageAsync(It.IsAny<NotificationMessage>()))
                .Returns(Task.CompletedTask);

            // Act
            await _sut!.PostEmailWithFileNotificationAsync(model);

            // Assert
            model.Content.Should().ContainKey("FileName").WhoseValue.Should().Be(fileName);
            model.Content.Should().ContainKey("FileId").WhoseValue.Should().Be(fileId);
        }

        [TestMethod]
        public async Task PostEmailWithFileNotificationAsync_FileValidatorThrows_PropagatesException()
        {
            // Arrange
            var organisationId = _fixture!.Create<Guid>();
            var file = CreateFormFile("test content", "test.pdf");
            var content = new Dictionary<string, object>
            {
                ["TemplateId"] = Guid.NewGuid().ToString(),
                ["EmailAddress"] = "test@test.com"
            };

            var model = new PostEmailWithFileNotificationRequestModel
            {
                organisationId = organisationId,
                SendType = GovNotifyAPIKeyType.PretendToSend,
                Content = content,
                File = file
            };

            var validationError = new StatusCodeException(Error.Validation, "Invalid file type", null, 415);
            _fileValidator!.Setup(v => v.Validate(file)).Throws(validationError);

            // Act & Assert
            await Assert.ThrowsExceptionAsync<StatusCodeException>(
                async () => await _sut!.PostEmailWithFileNotificationAsync(model));

            _fileValidator.Verify(v => v.Validate(file), Times.Once);
            _fileStorage!.Verify(fs => fs.UploadAttachmentAsync(It.IsAny<Stream>()), Times.Never);
            _messagingClient!.Verify(m => m.SendNotificationMessageAsync(It.IsAny<NotificationMessage>()), Times.Never);
        }

        [TestMethod]
        public async Task PostEmailWithFileNotificationAsync_UploadsFileToStorage()
        {
            // Arrange
            var organisationId = _fixture!.Create<Guid>();
            var fileContent = "test file content";
            var file = CreateFormFile(fileContent, "test.pdf");
            var content = new Dictionary<string, object>
            {
                ["TemplateId"] = Guid.NewGuid().ToString(),
                ["EmailAddress"] = "test@test.com"
            };

            var model = new PostEmailWithFileNotificationRequestModel
            {
                organisationId = organisationId,
                SendType = GovNotifyAPIKeyType.PretendToSend,
                Content = content,
                File = file
            };

            Stream? capturedStream = null;
            _fileValidator!.Setup(v => v.Validate(file));
            _fileStorage!.Setup(fs => fs.UploadAttachmentAsync(It.IsAny<Stream>()))
                .Callback<Stream>(s =>
                {
                    capturedStream = new MemoryStream();
                    s.CopyTo(capturedStream);
                })
                .ReturnsAsync("file-id");
            _messagingClient!.Setup(m => m.SendNotificationMessageAsync(It.IsAny<NotificationMessage>()))
                .Returns(Task.CompletedTask);

            // Act
            await _sut!.PostEmailWithFileNotificationAsync(model);

            // Assert
            capturedStream.Should().NotBeNull();
            capturedStream!.Seek(0, SeekOrigin.Begin);
            using var reader = new StreamReader(capturedStream);
            var uploadedContent = await reader.ReadToEndAsync();
            uploadedContent.Should().Be(fileContent);
        }

        [TestMethod]
        public async Task PostEmailWithFileNotificationAsync_WithNullSendType_UsesDefaultSendType()
        {
            // Arrange
            var organisationId = _fixture!.Create<Guid>();
            var file = CreateFormFile("test content", "test.pdf");
            var content = new Dictionary<string, object>
            {
                ["TemplateId"] = Guid.NewGuid().ToString(),
                ["EmailAddress"] = "test@test.com"
            };

            var model = new PostEmailWithFileNotificationRequestModel
            {
                organisationId = organisationId,
                SendType = null,
                Content = content,
                File = file
            };

            _fileValidator!.Setup(v => v.Validate(file));
            _fileStorage!.Setup(fs => fs.UploadAttachmentAsync(It.IsAny<Stream>())).ReturnsAsync("file-id");
            _messagingClient!.Setup(m => m.SendNotificationMessageAsync(It.IsAny<NotificationMessage>()))
                .Returns(Task.CompletedTask);

            // Act
            await _sut!.PostEmailWithFileNotificationAsync(model);

            // Assert
            _messagingClient.Verify(m => m.SendNotificationMessageAsync(
                It.Is<NotificationMessage>(n => n.APIKeyType == null)), Times.Once);
        }

        [TestMethod]
        public async Task PostEmailWithFileNotificationAsync_CreatesNotificationWithCorrectChannel()
        {
            // Arrange
            var organisationId = _fixture!.Create<Guid>();
            var file = CreateFormFile("test content", "test.pdf");
            var content = new Dictionary<string, object>
            {
                ["TemplateId"] = Guid.NewGuid().ToString(),
                ["EmailAddress"] = "test@test.com"
            };

            var model = new PostEmailWithFileNotificationRequestModel
            {
                organisationId = organisationId,
                SendType = GovNotifyAPIKeyType.PretendToSend,
                Content = content,
                File = file
            };

            NotificationMessage? capturedMessage = null;
            _fileValidator!.Setup(v => v.Validate(file));
            _fileStorage!.Setup(fs => fs.UploadAttachmentAsync(It.IsAny<Stream>())).ReturnsAsync("file-id");
            _messagingClient!.Setup(m => m.SendNotificationMessageAsync(It.IsAny<NotificationMessage>()))
                .Callback<NotificationMessage>(m => capturedMessage = m)
                .Returns(Task.CompletedTask);

            // Act
            await _sut!.PostEmailWithFileNotificationAsync(model);

            // Assert
            capturedMessage.Should().NotBeNull();
            capturedMessage!.Type.Should().Be(NotificationType.Email);
            var payload = IPOJsonSerialization.Deserialize<Dictionary<string, object>>(capturedMessage.PayLoad!);
            payload.Should().ContainKey("FileName");
            payload.Should().ContainKey("FileId");
        }

        #endregion

        #region Precompiled Letter Tests

        [TestMethod]
        [DataRow(null, "SecondClass", DisplayName = "DefaultsToSecondClass_WhenNull")]
        [DataRow(PrecompiledLetterPostage.FirstClass, "FirstClass", DisplayName = "UsesFirstClass_WhenSpecified")]
        [DataRow(PrecompiledLetterPostage.SecondClass, "SecondClass", DisplayName = "UsesSecondClass_WhenSpecified")]
        public async Task PostPrecompiledLetterRequestAsync_SetsCorrectPostage(
            PrecompiledLetterPostage? postage, string expectedPostageName)
        {
            // Arrange
            var organisationId = _fixture.Create<Guid>();
            var file = CreateFormFile("x", "letter.pdf");
            var model = new PrecompiledLetterRequestModel
            {
                organisationId = organisationId,
                File = file,
                SendType = GovNotifyAPIKeyType.PretendToSend,
                Postage = postage
            };

            _fileValidator!.Setup(v => v.ValidatePrecompiledLetter(file));
            _fileStorage!.Setup(fs => fs.UploadAttachmentAsync(It.IsAny<Stream>())).ReturnsAsync("blob-id");
            _messagingClient!.Setup(m => m.SendNotificationMessageAsync(It.IsAny<NotificationMessage>()))
                .Returns(Task.CompletedTask);

            // Act
            var id = await _sut!.PostPrecompiledLetterRequestAsync(model);

            // Assert
            id.Should().NotBe(Guid.Empty);
            _messagingClient.Verify(m => m.SendNotificationMessageAsync(It.Is<NotificationMessage>(nm =>
                nm.OrganisationId == organisationId &&
                nm.Type == NotificationType.PrecompiledLetter &&
                IPOJsonSerialization.Deserialize<Dictionary<string, object?>>(nm.PayLoad!)!["Postage"]!.ToString() == expectedPostageName
            )), Times.Once);
        }

        #endregion

        #region Template Preview Tests

        [TestMethod]
        public async Task GenerateTemplatePreviewAsync_ProxiesToNotifyGateway()
        {
            // Arrange
            var request = _fixture.Create<TemplatePreviewRequest>();
            var expected = _fixture.Create<TemplatePreviewResponse>();

            _notifyGateway!.Setup(g => g.GenerateTemplatePreviewAsync(request))
                .ReturnsAsync(expected);

            // Act
            var result = await _sut!.GenerateTemplatePreviewAsync(request);

            // Assert
            result.Should().Be(expected);
            _notifyGateway.Verify(g => g.GenerateTemplatePreviewAsync(request), Times.Once);
        }

        #endregion

        #region Process Notification Tests

        [TestMethod]
        public async Task ProcessNotificationMessageAsync_API_StoresMessageWithOwnerAndDeliveredConfirmed()
        {
            // Arrange
            var apiPayload = new
            {
                Email = "test@test.com",
                TemplateId = Guid.NewGuid(),
                Personalisation = new Dictionary<string, object>()
            };
            var envelope = CreateNotificationMessage(NotificationType.API, apiPayload);
            var json = IPOJsonSerialization.Serialize(envelope);

            SetupRepositoryStore();

            // Act
            await _sut!.ProcessNotificationMessageAsync(json, NotificationType.API);

            // Assert
            _repository!.Verify(r => r.StoreAsync(It.Is<NotificationMessage>(m =>
                m.Status == NotificationStatus.DeliveredConfirmed &&
                m.Owner != null &&
                m.Owner.Email == apiPayload.Email &&
                m.Owner.Id == envelope.OrganisationId), null!), Times.Once);
        }

        [TestMethod]
        public async Task ProcessNotificationMessageAsync_Email_WithoutFile_CallsNotifyAndStoresWithExternalId()
        {
            // Arrange
            var emailPayload = new
            {
                EmailAddress = "test@test.com",
                TemplateId = Guid.NewGuid(),
                Personalisation = new Dictionary<string, object>()
            };
            var envelope = CreateNotificationMessage(NotificationType.Email, emailPayload);
            var json = IPOJsonSerialization.Serialize(envelope);

            _notifyGateway!.Setup(g => g.SendEmailAsync(It.IsAny<EmailContent>(), It.IsAny<byte[]>(), It.IsAny<string>()))
                .ReturnsAsync("ext-123");
            SetupRepositoryStore("ext-123");

            // Act
            await _sut!.ProcessNotificationMessageAsync(json, NotificationType.Email);

            // Assert
            _repository!.Verify(r => r.StoreAsync(It.IsAny<NotificationMessage>(), "ext-123"), Times.Once);
            _fileStorage!.Verify(fs => fs.GetFileStreamAsync(It.IsAny<string>()), Times.Never);
            _notifyGateway.Verify(g => g.SendEmailAsync(It.IsAny<EmailContent>(), It.IsAny<byte[]>(), It.IsAny<string>()), Times.Once);
        }

        [TestMethod]
        public async Task ProcessNotificationMessageAsync_Email_WithFile_CallsFileStorageNotifyAndStores()
        {
            // Arrange
            var emailPayload = new
            {
                EmailAddress = "test@test.com",
                TemplateId = Guid.NewGuid(),
                FileId = "file-abc",
                Personalisation = new Dictionary<string, object>()
            };
            var envelope = CreateNotificationMessage(NotificationType.Email, emailPayload);
            var json = IPOJsonSerialization.Serialize(envelope);

            _fileStorage!.Setup(fs => fs.GetFileStreamAsync("file-abc"))
                .ReturnsAsync(new MemoryStream(new byte[] { 1, 2, 3 }));
            _notifyGateway!.Setup(g => g.SendEmailAsync(It.IsAny<EmailContent>(), It.IsAny<byte[]>(), It.IsAny<string>()))
                .ReturnsAsync("ext-456");
            SetupRepositoryStore("ext-456");

            // Act
            await _sut!.ProcessNotificationMessageAsync(json, NotificationType.Email);

            // Assert
            _fileStorage.Verify(fs => fs.GetFileStreamAsync("file-abc"), Times.Once);
            _notifyGateway.Verify(g => g.SendEmailAsync(It.IsAny<EmailContent>(),
                It.Is<byte[]>(b => b.SequenceEqual(new byte[] { 1, 2, 3 })), It.IsAny<string>()), Times.Once);
            _repository!.Verify(r => r.StoreAsync(It.IsAny<NotificationMessage>(), "ext-456"), Times.Once);
        }

        [TestMethod]
        public async Task ProcessNotificationMessageAsync_Sms_CallsNotifyAndStoresExternalId()
        {
            // Arrange
            var smsPayload = new
            {
                MobileNumber = "07700000000",
                TemplateId = Guid.NewGuid(),
                Personalisation = new Dictionary<string, object>()
            };
            var envelope = CreateNotificationMessage(NotificationType.SMS, smsPayload);
            var json = IPOJsonSerialization.Serialize(envelope);

            _notifyGateway!.Setup(g => g.SendSmsAsync(It.IsAny<SmsContent>(), It.IsAny<string>()))
                .ReturnsAsync("ext-sms-1");
            SetupRepositoryStore("ext-sms-1");

            // Act
            await _sut!.ProcessNotificationMessageAsync(json, NotificationType.SMS);

            // Assert
            _notifyGateway.Verify(g => g.SendSmsAsync(It.IsAny<SmsContent>(), It.IsAny<string>()), Times.Once);
            _repository!.Verify(r => r.StoreAsync(It.IsAny<NotificationMessage>(), "ext-sms-1"), Times.Once);
        }

        [TestMethod]
        public async Task ProcessNotificationMessageAsync_WithNullPayload_ThrowsStatusCodeException()
        {
            // Arrange & Act & Assert
            var exception = await Assert.ThrowsExceptionAsync<StatusCodeException>(
                async () => await _sut!.ProcessNotificationMessageAsync(null!, NotificationType.API));

            // Assert
            Assert.IsTrue(exception.Error.Description!.Contains("Message payload cannot be null or empty"));
        }

        [TestMethod]
        [DataRow(NotificationType.Letter, DisplayName = "Letter_HappyPath")]
        [DataRow(NotificationType.PrecompiledLetter, DisplayName = "PrecompiledLetter_HappyPath")]
        public async Task ProcessLetterNotificationAsync_HappyPath(NotificationType letterType)
        {
            // Arrange
            object payload = letterType == NotificationType.Letter
                ? new LetterContent { TemplateId = Guid.NewGuid().ToString(), Personalisation = new Dictionary<string, object> { ["Name"] = "A" } }
                : new PrecompiledLetterContent { FileId = "pdf1", ClientReference = Guid.NewGuid() };

            var msg = CreateNotificationMessage(letterType, payload);
            var json = IPOJsonSerialization.Serialize(msg);

            if (letterType == NotificationType.Letter)
            {
                _notifyGateway!.Setup(g => g.SendLetterAsync(It.IsAny<LetterContent>(), It.IsAny<string>()))
                    .ReturnsAsync("ext-letter");
            }
            else
            {
                _fileStorage!.Setup(fs => fs.GetFileStreamAsync("pdf1"))
                    .ReturnsAsync(new MemoryStream(new byte[] { 9 }));
                _notifyGateway!.Setup(g => g.SendPrecompiledLetterAsync(It.IsAny<PrecompiledLetterContent>(),
                    It.IsAny<byte[]>(), It.IsAny<string>()))
                    .ReturnsAsync("ext-preletter");
            }

            SetupRepositoryStore(It.IsAny<string>());

            // Act
            await _sut!.ProcessNotificationMessageAsync(json, letterType);

            // Assert
            _repository!.Verify(r => r.StoreAsync(It.IsAny<NotificationMessage>(), It.IsAny<string>()), Times.Once);
        }

        #endregion

        #region Error Handling Tests

        [TestMethod]
        [DataRow("403", NotificationStatus.Rejected, DisplayName = "Error403_SetsRejected")]
        [DataRow("429", NotificationStatus.Rejected, DisplayName = "Error429_SetsRejected")]
        [DataRow("400", NotificationStatus.UserError, DisplayName = "Error400_SetsUserError")]
        [DataRow("404", NotificationStatus.UserError, DisplayName = "Error404_SetsUserError")]
        [DataRow("500", NotificationStatus.SystemFailure, DisplayName = "Error500_SetsSystemFailure")]
        public async Task ProcessEmailNotificationAsync_GovNotifyError_SetsCorrectStatus(
            string statusCode, NotificationStatus expectedStatus)
        {
            // Arrange
            var content = new EmailContent { EmailAddress = "x@x.com", TemplateId = Guid.NewGuid().ToString() };
            var msg = CreateNotificationMessage(NotificationType.Email, content);
            var json = IPOJsonSerialization.Serialize(msg);

            _notifyGateway!.Setup(g => g.SendEmailAsync(It.IsAny<EmailContent>(), It.IsAny<byte[]>(), It.IsAny<string>()))
                .ThrowsAsync(new Exception($"Status code {statusCode}") { Source = "GovukNotify" });
            SetupRepositoryStore();

            // Act
            await _sut!.ProcessNotificationMessageAsync(json, NotificationType.Email);

            // Assert
            _repository!.Verify(r => r.StoreAsync(
                It.Is<NotificationMessage>(m => m.Status == expectedStatus && m.Errors == $"Status code {statusCode}"),
                null!), Times.Once);
        }

        [TestMethod]
        public async Task ProcessEmailNotificationAsync_ErrorWithInnerGovNotifySource_SetsStatusAndErrors()
        {
            // Arrange
            var content = new EmailContent { EmailAddress = "x@x.com", TemplateId = Guid.NewGuid().ToString() };
            var msg = CreateNotificationMessage(NotificationType.Email, content);
            var json = IPOJsonSerialization.Serialize(msg);

            var inner = new Exception("Status code 500") { Source = "GovukNotify" };
            _notifyGateway!.Setup(g => g.SendEmailAsync(It.IsAny<EmailContent>(), It.IsAny<byte[]>(), It.IsAny<string>()))
                .ThrowsAsync(new Exception("outer", inner));
            SetupRepositoryStore();

            // Act
            await _sut!.ProcessNotificationMessageAsync(json, NotificationType.Email);

            // Assert
            _repository!.Verify(r => r.StoreAsync(It.Is<NotificationMessage>(m =>
                m.Status == NotificationStatus.SystemFailure &&
                m.Errors == "Status code 500"), null!), Times.Once);
        }

        [TestMethod]
        public async Task ProcessSmsNotificationAsync_ErrorGovNotify_MapsToUserError_On400()
        {
            // Arrange
            var content = new SmsContent { MobileNumber = "077", TemplateId = Guid.NewGuid().ToString() };
            var msg = CreateNotificationMessage(NotificationType.SMS, content);
            var json = IPOJsonSerialization.Serialize(msg);

            _notifyGateway!.Setup(g => g.SendSmsAsync(It.IsAny<SmsContent>(), It.IsAny<string>()))
                .ThrowsAsync(new Exception("Status code 400") { Source = "GovukNotify" });
            SetupRepositoryStore();

            // Act
            await _sut!.ProcessNotificationMessageAsync(json, NotificationType.SMS);

            // Assert
            _repository!.Verify(r => r.StoreAsync(
                It.Is<NotificationMessage>(m => m.Status == NotificationStatus.UserError && m.Errors == "Status code 400"),
                null!), Times.Once);
        }

        [TestMethod]
        public async Task ProcessLetterNotificationAsync_ErrorGovNotify_SetsUserError()
        {
            // Arrange
            var payload = new LetterContent
            {
                TemplateId = Guid.NewGuid().ToString(),
                Personalisation = new Dictionary<string, object> { ["Name"] = "A" }
            };
            var msg = CreateNotificationMessage(NotificationType.Letter, payload);
            var json = IPOJsonSerialization.Serialize(msg);

            _notifyGateway!.Setup(g => g.SendLetterAsync(It.IsAny<LetterContent>(), It.IsAny<string>()))
                .ThrowsAsync(new Exception("Status code 404") { Source = "GovukNotify" });
            SetupRepositoryStore();

            // Act
            await _sut!.ProcessNotificationMessageAsync(json, NotificationType.Letter);

            // Assert
            _repository!.Verify(r => r.StoreAsync(
                It.Is<NotificationMessage>(m => m.Status == NotificationStatus.UserError && m.Errors == "Status code 404"),
                null!), Times.Once);
        }

        #endregion

        #region Status Update Tests

        [TestMethod]
        public async Task UpdateNotificationStatusesAsync_NoMessages_ReturnsEarly()
        {
            // Arrange
            _repository!.Setup(r => r.GetMessagesForStatusUpdateAsync())
                .ReturnsAsync(Enumerable.Empty<NotificationMessage>());

            // Act
            await _sut!.UpdateNotificationStatusesAsync();

            // Assert
            _notifyGateway!.Verify(g => g.CheckStatusUpdatesAsync(It.IsAny<IEnumerable<NotificationMessage>>()), Times.Never);
            _repository!.Verify(r => r.SaveChangesAsync(), Times.Never);
            _fileStorage!.Verify(fs => fs.DeleteAttachmentsAsync(It.IsAny<IEnumerable<string>>()), Times.Never);
        }

        [TestMethod]
        public async Task UpdateNotificationStatusesAsync_WithMessages_SavesAndDeletesAttachments()
        {
            // Arrange
            var msgs = new[]
            {
                new NotificationMessage
                {
                    PayLoad = IPOJsonSerialization.Serialize(new EmailContent { FileId = "f1", EmailAddress = "test@test.com", TemplateId = Guid.NewGuid().ToString() }),
                    Type = NotificationType.Email,
                    Status = NotificationStatus.DeliveredConfirmed
                },
                new NotificationMessage
                {
                    PayLoad = IPOJsonSerialization.Serialize(new EmailContent { FileId = "", EmailAddress = "test2@test.com", TemplateId = Guid.NewGuid().ToString() }),
                    Type = NotificationType.Email,
                    Status = NotificationStatus.DeliveredConfirmed
                },
                new NotificationMessage
                {
                    PayLoad = IPOJsonSerialization.Serialize(new SmsContent { MobileNumber = "07700000000", TemplateId = Guid.NewGuid().ToString() }),
                    Type = NotificationType.SMS,
                    Status = NotificationStatus.DeliveredConfirmed
                }
            };

            IEnumerable<string>? capturedIds = null;

            _repository!.Setup(r => r.GetMessagesForStatusUpdateAsync()).ReturnsAsync(msgs);
            _notifyGateway!.Setup(g => g.CheckStatusUpdatesAsync(msgs)).Returns(Task.CompletedTask);
            _repository.Setup(r => r.SaveChangesAsync()).Returns(Task.CompletedTask);
            _fileStorage!.Setup(fs => fs.DeleteAttachmentsAsync(It.IsAny<IEnumerable<string>>()))
                .Callback<IEnumerable<string>>(ids => capturedIds = ids.ToList())
                .Returns(Task.CompletedTask);

            // Act
            await _sut!.UpdateNotificationStatusesAsync();

            // Assert
            _notifyGateway.Verify(g => g.CheckStatusUpdatesAsync(It.Is<IEnumerable<NotificationMessage>>(m => m.SequenceEqual(msgs))), Times.Once);
            _repository.Verify(r => r.SaveChangesAsync(), Times.Once);
            _fileStorage.Verify(fs => fs.DeleteAttachmentsAsync(It.IsAny<IEnumerable<string>>()), Times.Once);

            capturedIds.Should().NotBeNull();
            capturedIds.Should().HaveCount(2);
            capturedIds.Should().Contain("f1");
            capturedIds.Should().Contain("");
        }

        #endregion
    }
}