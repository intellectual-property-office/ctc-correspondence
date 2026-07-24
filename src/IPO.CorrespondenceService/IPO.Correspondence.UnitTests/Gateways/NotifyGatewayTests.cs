using AwesomeAssertions;
using IPO.Common.Infrastructure;
using IPO.Correspondence.GovNotify;
using IPO.Correspondence.Interfaces.Gateways;
using IPO.Correspondence.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Notify.Interfaces;
using Notify.Models.Responses;
using Microsoft.Extensions.Configuration;
using IPO.Correspondence.Services.Validation;

namespace IPO.Correspondence.UnitTests.GovNotify
{
	[TestClass]
    public class NotifyGatewayTests
    {
        private readonly Mock<IConfiguration> _functionConfiguration;

        public NotifyGatewayTests()
        {
            Error.Add(Error.Create<CorrespondenceServiceValidator>("E-007"));
            Error.Add(Error.Create<NotifyGateway>("E-008"));

            _functionConfiguration = new Mock<IConfiguration>(MockBehavior.Strict);
            _functionConfiguration.SetupGet(x => x[It.Is<string>(s => s == "GovNotifyApiKey_DefaultMethod")]).Returns("PretendToSend");
            _functionConfiguration.SetupGet(x => x[It.Is<string>(s => s == "GovNotifyApiKey_PretendToSend")]).Returns("1");
            _functionConfiguration.SetupGet(x => x[It.Is<string>(s => s == "GovNotifyApiKey_SendToListOnly")]).Returns("1");
            _functionConfiguration.SetupGet(x => x[It.Is<string>(s => s == "GovNotifyPassThroughURL")]).Returns("0");
            _functionConfiguration.SetupGet(x => x[It.Is<string>(s => s == "GovNotifyPassThroughKey")]).Returns("0");
            _functionConfiguration.SetupGet(x => x[It.Is<string>(s => s == "GovNotifyPassThroughVersion")]).Returns("0");

        }

        [TestMethod]
        public void SendEmailAsyncSuccessful()
        {
            // Arrange
            var asyncNotificationClient = new Mock<IAsyncNotificationClient>(MockBehavior.Strict);
            var correspondenceServiceValidator = new Mock<ICorrespondenceServiceValidator>(MockBehavior.Strict);            
          
            asyncNotificationClient.Setup(c => c.SendEmailAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<Dictionary<string, dynamic>>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>()))
                .ReturnsAsync(new EmailNotificationResponse { id = Guid.NewGuid().ToString() });
       
            var emailContent = new EmailContent 
            { 
                FileName = "file.csv", 
                Personalisation = new(), 
                FilePersonalisationName = string.Empty 
            };

            var fileAttachment = Array.Empty<byte>();
            var sut = new NotifyGateway(asyncNotificationClient.Object, correspondenceServiceValidator.Object, _functionConfiguration.Object);
       
            // Act
            var result = sut.SendEmailAsync(emailContent, fileAttachment, string.Empty);

            // Assert
            result.Should().NotBeNull();
            Guid.TryParse(result.Result, out _).Should().BeTrue();
        }

        [TestMethod]
        public void SendEmailWithSpecificSendTypeAsyncSuccessful()
        {
            // Arrange
            var asyncNotificationClient = new Mock<IAsyncNotificationClient>(MockBehavior.Strict);
            var correspondenceServiceValidator = new Mock<ICorrespondenceServiceValidator>(MockBehavior.Strict);
         
            asyncNotificationClient.Setup(c => c.SendEmailAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<Dictionary<string, dynamic>>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>()))
                .ReturnsAsync(new EmailNotificationResponse { id = Guid.NewGuid().ToString() });

            var emailContent = new EmailContent
            {
                FileName = "file.csv",
                Personalisation = new(),
                FilePersonalisationName = string.Empty
            };

            var fileAttachment = Array.Empty<byte>();
            var sut = new NotifyGateway(asyncNotificationClient.Object, correspondenceServiceValidator.Object, _functionConfiguration.Object, "SendToListOnly", asyncNotificationClient.Object);
         
            // Act
            var result = sut.SendEmailAsync(emailContent, fileAttachment, "SendToListOnly");

            // Assert
            result.Should().NotBeNull();
            Guid.TryParse(result.Result, out _).Should().BeTrue();
        }

        [TestMethod]
        public void SendLetterAsyncSuccessful()
        {
            // Arrange
            var asyncNotificationClient = new Mock<IAsyncNotificationClient>(MockBehavior.Strict);
            var correspondenceServiceValidator = new Mock<ICorrespondenceServiceValidator>(MockBehavior.Strict);        
            
            asyncNotificationClient.Setup(c => c.SendLetterAsync(
                It.IsAny<string>(),
                It.IsAny<Dictionary<string, dynamic>>(),
                It.IsAny<string>()))
                .ReturnsAsync(new LetterNotificationResponse { id = Guid.NewGuid().ToString() });

            var letterContent = new LetterContent
            {
                TemplateId = Guid.NewGuid().ToString(),
                Personalisation = new()
            };

            var sut = new NotifyGateway(asyncNotificationClient.Object, correspondenceServiceValidator.Object, _functionConfiguration.Object);

            // Act
            var result = sut.SendLetterAsync(letterContent, string.Empty);

            // Assert
            result.Should().NotBeNull();
            Guid.TryParse(result.Result, out _).Should().BeTrue();
        }

        [TestMethod]
        public void SendLetterWithSpecificSendTypeAsyncSuccessful()
        {
            // Arrange
            var asyncNotificationClient = new Mock<IAsyncNotificationClient>(MockBehavior.Strict);
            var correspondenceServiceValidator = new Mock<ICorrespondenceServiceValidator>(MockBehavior.Strict);
         
            asyncNotificationClient.Setup(c => c.SendLetterAsync(
                   It.IsAny<string>(),
                   It.IsAny<Dictionary<string, dynamic>>(),
                   It.IsAny<string>()))
                   .ReturnsAsync(new LetterNotificationResponse { id = Guid.NewGuid().ToString() });

            var letterContent = new LetterContent
            {
                TemplateId = Guid.NewGuid().ToString(),
                Personalisation = new()
            };

            var sut = new NotifyGateway(asyncNotificationClient.Object, correspondenceServiceValidator.Object, _functionConfiguration.Object, "SendToListOnly", asyncNotificationClient.Object);

            // Act
            var result = sut.SendLetterAsync(letterContent, "SendToListOnly");

            // Assert
            result.Should().NotBeNull();
            Guid.TryParse(result.Result, out _).Should().BeTrue();
        }

        [TestMethod]
        [DataRow(PrecompiledLetterPostage.FirstClass)]
        [DataRow(PrecompiledLetterPostage.SecondClass)]
        public void SendPrecompiledLetterAsyncSuccessful(PrecompiledLetterPostage postage)
        {

            // Arrange
            var asyncNotificationClient = new Mock<IAsyncNotificationClient>(MockBehavior.Strict);
            var correspondenceServiceValidator = new Mock<ICorrespondenceServiceValidator>(MockBehavior.Strict);

            asyncNotificationClient.Setup(c => c.SendPrecompiledLetterAsync(
                It.IsAny<string>(),
                It.IsAny<byte[]>(), 
                It.IsAny<string>()))
                .ReturnsAsync(new LetterNotificationResponse { id = Guid.NewGuid().ToString() });

            var precompiledLetterContent = new PrecompiledLetterContent
            {
                FileName = "test.pdf",
                FileId = "00000000-0000-0000-0000-000000000000",
                ClientReference = Guid.NewGuid(),
                Postage = postage
            };

            var pdfContent = Array.Empty<byte>();
            var sut = new NotifyGateway(asyncNotificationClient.Object, correspondenceServiceValidator.Object, _functionConfiguration.Object);

            // Act
            var result = sut.SendPrecompiledLetterAsync(precompiledLetterContent, pdfContent, string.Empty);

            // Assert
            result.Should().NotBeNull();
            Guid.TryParse(result.Result, out _).Should().BeTrue();
        }

        [TestMethod]
        public void SendSmsAsyncSuccessful()
        {
            // Arrange
            var asyncNotificationClient = new Mock<IAsyncNotificationClient>(MockBehavior.Strict);
            var correspondenceServiceValidator = new Mock<ICorrespondenceServiceValidator>(MockBehavior.Strict);
          
            asyncNotificationClient.Setup(c => c.SendSmsAsync(
                It.IsAny<string>(), 
                It.IsAny<string>(),
                It.IsAny<Dictionary<string, dynamic>>(),
                It.IsAny<string>(),
                It.IsAny<string>()))
                .ReturnsAsync(new SmsNotificationResponse { id = Guid.NewGuid().ToString() });

            var smsContent = new SmsContent
            {
                MobileNumber = string.Empty,
                TemplateId = Guid.NewGuid().ToString(),
                Personalisation = new()
            };
                       
            var sut = new NotifyGateway(asyncNotificationClient.Object, correspondenceServiceValidator.Object, _functionConfiguration.Object);

            // Act
            var result = sut.SendSmsAsync(smsContent, string.Empty);

            // Assert
            result.Should().NotBeNull();
            Guid.TryParse(result.Result, out _).Should().BeTrue();
        }

        [TestMethod]
        public void SendSmsWithSpecificSendTypeAsyncSuccessful()
        {
            // Arrange
            var asyncNotificationClient = new Mock<IAsyncNotificationClient>(MockBehavior.Strict);
            var correspondenceServiceValidator = new Mock<ICorrespondenceServiceValidator>(MockBehavior.Strict);
          
            asyncNotificationClient.Setup(c => c.SendSmsAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<Dictionary<string, dynamic>>(),
                It.IsAny<string>(),
                It.IsAny<string>()))
                .ReturnsAsync(new SmsNotificationResponse { id = Guid.NewGuid().ToString() });

            var smsContent = new SmsContent
            {
                MobileNumber = string.Empty,
                TemplateId = Guid.NewGuid().ToString(),
                Personalisation = new()
            };

            var sut = new NotifyGateway(asyncNotificationClient.Object, correspondenceServiceValidator.Object, _functionConfiguration.Object, "SendToListOnly", asyncNotificationClient.Object);

            // Act
            var result = sut.SendSmsAsync(smsContent, "SendToListOnly");

            // Assert
            result.Should().NotBeNull();
            Guid.TryParse(result.Result, out _).Should().BeTrue();
        }

        [TestMethod]
        [ExpectedException(typeof(StatusCodeException))]
        public void DetectInvalidSendType()
        {
            // Arrange

            var correspondenceServiceValidator = new CorrespondenceServiceValidator(_functionConfiguration.Object);
            var sendType = GovNotifyAPIKeyType.SendToAnyone;

            _functionConfiguration.SetupGet(x => x[It.Is<string>(s => s == "GovNotifyApiKey_AllowedMethods")]).Returns("PretendToSend, SendToListOnly");

            // Act and Assert
            correspondenceServiceValidator.VerifySpecifiedSendType(sendType);
        }

        [TestMethod]
        public void AllowValidSendType()
        {
            // Arrange
            var correspondenceServiceValidator = new CorrespondenceServiceValidator(_functionConfiguration.Object);
            var sendType = GovNotifyAPIKeyType.SendToListOnly;

            _functionConfiguration.SetupGet(x => x[It.Is<string>(s => s == "GovNotifyApiKey_AllowedMethods")]).Returns("PretendToSend, SendToListOnly");

            // Act and Assert
            correspondenceServiceValidator.VerifySpecifiedSendType(sendType);                       
        }

        [DataTestMethod]
        [DataRow("WithPersonalisation")]  
        [DataRow("WithoutPersonalisation")]  
        public void GenerateTemplatePreviewAsyncSuccessful(string personalisationScenario)
        {
            // Arrange
            var asyncNotificationClient = new Mock<IAsyncNotificationClient>(MockBehavior.Strict);
            var correspondenceServiceValidator = new Mock<ICorrespondenceServiceValidator>(MockBehavior.Strict);
            var functionConfiguration = new Mock<IConfiguration>(MockBehavior.Strict);
            var templateIdData = Guid.NewGuid().ToString();

            object personalisation = personalisationScenario switch
            {
                "WithPersonalisation" => new Dictionary<string, object>
                {
                    { "TemplatePlaceholderFieldNameA", "DataA" },
                    { "TemplatePlaceholderFieldNameB", "DataB" }
                },
                _ => new { }
            };

            var testRequestContent = new Dictionary<string, object>
            {
                { "templateId", templateIdData },
                { "returnAsHtml", false },
                { "personalisation", personalisation }
            };

            asyncNotificationClient.Setup(c => c.GenerateTemplatePreviewAsync(
                  It.IsAny<string>(),
                  It.IsAny<Dictionary<string, dynamic>>()))
                .ReturnsAsync(new Notify.Models.Responses.TemplatePreviewResponse
                {
                    type = "Email",
                    id = templateIdData,
                    version = 1,
                    subject = "TestSubject",
                    body = "TestBody"
                });

            correspondenceServiceValidator.Setup(c => c.ValidateTemplatePreviewRequest(
               It.IsAny<TemplatePreviewRequest>()))
               .Returns(new Dictionary<string, object>(testRequestContent, StringComparer.OrdinalIgnoreCase));

            var _notifyGateway = new NotifyGateway(asyncNotificationClient.Object, correspondenceServiceValidator.Object, functionConfiguration.Object);

            // Act
            var result = _notifyGateway.GenerateTemplatePreviewAsync(new TemplatePreviewRequest { Content = testRequestContent });

            // Assert
            result.Should().NotBeNull();
            result.IsCanceled.Should().BeFalse();
            result.IsFaulted.Should().BeFalse();
            result.Exception.Should().BeNull();
            result.IsCompleted.Should().BeTrue();
        }

        [TestMethod]
        public void GenerateTemplatePreviewAsyncFail()
        {
            // Arrange
            var asyncNotificationClient = new Mock<IAsyncNotificationClient>(MockBehavior.Strict);
            var correspondenceServiceValidator = new Mock<ICorrespondenceServiceValidator>(MockBehavior.Strict);
            var functionConfiguration = new Mock<IConfiguration>(MockBehavior.Strict);
            var testRequestContent = new Dictionary<string, object>()
                {
                          {"templateId", "00000000-0000-0000-0000-00000000"},
                          {"returnAsHtml", false},
                          {"personalisation", "{\r\n \"TemplatePlaceholderFieldNameA\": \"DataA\",\r\n \"TemplatePlaceholderFieldNameB\": \"DataB\"\r\n}"}
                };

            asyncNotificationClient.Setup(c => c.GenerateTemplatePreviewAsync(
                  It.IsAny<string>(),
                  It.IsAny<Dictionary<string, dynamic>>())).Throws(new Exception("Status code 400. Error: {\"errors\":[{\"error\":\"ValidationError\",\"message\":\"id is not a valid UUID\"}],\"status_code\":400}\r\n, Exception: Status code 400. The following errors occured [\r\n  {\r\n    \"error\": \"ValidationError\",\r\n    \"message\": \"id is not a valid UUID\"\r\n  }\r\n]")); 

            correspondenceServiceValidator.Setup(c => c.ValidateTemplatePreviewRequest(
               It.IsAny<TemplatePreviewRequest>())).Returns(new Dictionary<string, object>(testRequestContent, StringComparer.OrdinalIgnoreCase));

            var _notifyGateway = new NotifyGateway(asyncNotificationClient.Object, correspondenceServiceValidator.Object, functionConfiguration.Object);

            var templatePreviewContent = new TemplatePreviewRequest
            {
                Content = testRequestContent
            };
          
            // Act
            var result = _notifyGateway.GenerateTemplatePreviewAsync(templatePreviewContent);

            // Assert
            result.Exception.Should().NotBeNull();
            result.IsFaulted.Should().BeTrue();               
            result.IsCompleted.Should().BeTrue();
            result.IsCompletedSuccessfully.Should().BeFalse();
            result.Exception!.InnerException!.Message.Should().Be("The NotifyGateway encountered an error.Status code 400. " +
               "Error: {\"errors\":[{\"error\":\"ValidationError\",\"message\":\"id is not a valid UUID\"}],\"status_code\":400}\r\n, " +
               "Exception: Status code 400. The following errors occured " +
               "[\r\n  {\r\n    \"error\": \"ValidationError\",\r\n    \"message\": \"id is not a valid UUID\"\r\n  }\r\n]");
        }

        [TestMethod]
        [ExpectedException(typeof(StatusCodeException))]
        public void DetectInvalidTemplatePreviewParameter()
        {
            // Arrange
            var correspondenceServiceValidator = new CorrespondenceServiceValidator();

            var templatePreviewRequest = new TemplatePreviewRequest
            {
                Content = new Dictionary<string, object>()
                {
                          {"InvalidId", "00000000-0000-0000-0000-00000000"}
                }
            };

            // Act and Assert
            correspondenceServiceValidator.ValidateTemplatePreviewRequest(templatePreviewRequest);
        }

        [TestMethod]
        public void HandleMissingOptionalTemplatePreviewParameters()
        {
            // Arrange
            var correspondenceServiceValidator = new CorrespondenceServiceValidator();

            var templatePreviewRequest = new TemplatePreviewRequest
            {
                Content = new Dictionary<string, object>()
                {
                          {"templateId", "00000000-0000-0000-0000-00000000"}
                }
            };

            // Act 
           var result = correspondenceServiceValidator.ValidateTemplatePreviewRequest(templatePreviewRequest);

            //Assert
            result.Should().NotBeNull();
            result.Should().ContainKeys("templateId", "returnAsHtml", "personalisation");
            result.Should().ContainKey("templateId").WhoseValue.Equals("00000000-0000-0000-0000-00000000");
            result.Should().ContainKey("returnAsHtml").WhoseValue.Equals(false);
            result.Should().ContainKey("personalisation").WhoseValue.Equals(new { });

        }

        [TestMethod]
        [ExpectedException(typeof(StatusCodeException))]
        public void DetectInvalidReturnAsHTMLType()
        {
            // Arrange
            var correspondenceServiceValidator = new CorrespondenceServiceValidator();

            var templatePreviewRequest = new TemplatePreviewRequest
            {
                Content = new Dictionary<string, object>()
                {
                          {"templateId", "00000000-0000-0000-0000-00000000"},
                          {"returnAsHtml", "invalidtype"}

                }
            };

            // Act and Assert
            correspondenceServiceValidator.ValidateTemplatePreviewRequest(templatePreviewRequest);
        }


        [TestMethod]
        [ExpectedException(typeof(StatusCodeException))]
        public void DetectInvalidReturnAsHTMLValue()
        {
            // Arrange
            var correspondenceServiceValidator = new CorrespondenceServiceValidator();

            var templatePreviewRequest = new TemplatePreviewRequest
            {
                Content = new Dictionary<string, object>()
                {
                          {"templateId", "00000000-0000-0000-0000-00000000"},
                          {"returnAsHtml", 1}

                }
            };

            // Act and Assert
            correspondenceServiceValidator.ValidateTemplatePreviewRequest(templatePreviewRequest);
        }

        [TestMethod]
        public void DetectValidReturnAsHTMLValue()
        {
            // Arrange
            var correspondenceServiceValidator = new CorrespondenceServiceValidator();

            var templatePreviewRequest = new TemplatePreviewRequest
            {
                Content = new Dictionary<string, object>()
                {
                          {"templateId", "00000000-0000-0000-0000-00000000"},
                          {"returnAsHtml", true}

                }
            };

            // Act and Assert
            correspondenceServiceValidator.ValidateTemplatePreviewRequest(templatePreviewRequest);
        }

        [DataTestMethod]
        [DataRow("xxx", NotificationType.Email, "created", NotificationStatus.SendingToProvider)]
        [DataRow("xxx", NotificationType.Email, "sending", NotificationStatus.SendingToRecipient)]
        [DataRow("xxx", NotificationType.Email, "permanent-failure", NotificationStatus.UserError)]
        [DataRow("xxx", NotificationType.Email, "temporary-failure", NotificationStatus.Rejected)]
        [DataRow("xxx", NotificationType.Email, "technical-failure", NotificationStatus.SystemFailure)]
        [DataRow("xxx", NotificationType.Email, "delivered", NotificationStatus.DeliveredConfirmed)]
        [DataRow("xxx", NotificationType.Email, "Unknown Error", NotificationStatus.Unknown)]
        [DataRow(null, NotificationType.Email, null, NotificationStatus.Unknown)]
        [DataRow("xxx", NotificationType.SMS, "created", NotificationStatus.SendingToProvider)]
        [DataRow("xxx", NotificationType.SMS, "sending", NotificationStatus.SendingToRecipient)]
        [DataRow("xxx", NotificationType.SMS, "pending", NotificationStatus.SendingToRecipient)]
        [DataRow("xxx", NotificationType.SMS, "sent", NotificationStatus.DeliveredUnconfirmed)]
        [DataRow("xxx", NotificationType.SMS, "delivered", NotificationStatus.DeliveredConfirmed)]
        [DataRow("xxx", NotificationType.SMS, "permanent-failure", NotificationStatus.UserError)]
        [DataRow("xxx", NotificationType.SMS, "temporary-failure", NotificationStatus.Rejected)]
        [DataRow("xxx", NotificationType.SMS, "technical-failure", NotificationStatus.SystemFailure)]
        [DataRow("xxx", NotificationType.SMS, "Unknown Error", NotificationStatus.Unknown)]
        [DataRow(null, NotificationType.SMS, null, NotificationStatus.Unknown)]
        [DataRow("xxx", NotificationType.Letter, "accepted", NotificationStatus.SendingToRecipient)]
        [DataRow("xxx", NotificationType.Letter, "received", NotificationStatus.DeliveredUnconfirmed)]
        [DataRow("xxx", NotificationType.Letter, "pending-virus-check", NotificationStatus.SendingToProvider)]
        [DataRow("xxx", NotificationType.Letter, "technical-failure", NotificationStatus.SystemFailure)]
        [DataRow("xxx", NotificationType.Letter, "permanent-failure", NotificationStatus.UserError)]
        [DataRow("xxx", NotificationType.Letter, "validation-failed", NotificationStatus.ValidationFailed)]
        [DataRow("xxx", NotificationType.Letter, "virus-scan-failed", NotificationStatus.VirusScanFailed)]
        [DataRow("xxx", NotificationType.Letter, "cancelled", NotificationStatus.Cancelled)]
        [DataRow("xxx", NotificationType.Letter, "Unknown Error", NotificationStatus.Unknown)]
        [DataRow(null, NotificationType.Letter, null, NotificationStatus.Unknown)]
        public void CheckStatusUpdatesAsyncLogicBranches(
            string externalId,
            NotificationType messageType,
            string mockedGovNotifyNotificationStatus,
            NotificationStatus expectedStatus)
        {
            // Arrange
            var asyncNotificationClient = new Mock<IAsyncNotificationClient>(MockBehavior.Strict);
            var correspondenceServiceValidator = new Mock<ICorrespondenceServiceValidator>(MockBehavior.Strict);
            var functionConfiguration = new Mock<IConfiguration>(MockBehavior.Strict);

            asyncNotificationClient.Setup(c => c.GetNotificationByIdAsync(It.IsAny<string>()))
                .ReturnsAsync(new Notify.Models.Notification { status = mockedGovNotifyNotificationStatus });

            var notificationMessages = new NotificationMessage[]
            {
                new NotificationMessage 
                {
                    ExternalId = externalId,
                    Type = messageType
                }
            };

            var sut = new NotifyGateway(asyncNotificationClient.Object, correspondenceServiceValidator.Object, _functionConfiguration.Object);

            // Act
            var result = sut.CheckStatusUpdatesAsync(notificationMessages);

            // Assert
            result.Should().NotBeNull();
            notificationMessages[0].Status.Should().Be(expectedStatus);
        }

		[TestMethod]
        public void CheckStatusUpdatesAsyncWhenNotifyThrowsExceptionSetsMessageStatusToUnknown()
		{
			// Arrange
			var asyncNotificationClient = new Mock<IAsyncNotificationClient>(MockBehavior.Strict);
			Exception exception = new Exception() { Source = "GovukNotify" };

			asyncNotificationClient.Setup(r => r.GetNotificationByIdAsync(It.IsAny<string>())).Throws(exception);
			var sut = new NotifyGateway(asyncNotificationClient.Object, null!, null!);

            var notificationMessages = new NotificationMessage[]
            {
                new NotificationMessage
                {
                    ExternalId = "123abc",
                    Type = 0
                }
            };

            // Act
            var result = sut.CheckStatusUpdatesAsync(notificationMessages);

			// Assert
			result.Should().NotBeNull();
			notificationMessages[0].Status.Should().Be(NotificationStatus.Unknown);
		}
	}
}
