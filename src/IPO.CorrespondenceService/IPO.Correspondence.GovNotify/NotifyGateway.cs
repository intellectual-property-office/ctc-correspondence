using IPO.Common.Infrastructure;
using IPO.Correspondence.Interfaces.Gateways;
using IPO.Correspondence.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Notify.Client;
using Notify.Interfaces;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace IPO.Correspondence.GovNotify
{
    public class NotifyGateway : INotifyGateway
    {
        private readonly IAsyncNotificationClient _client; 
        private static IAsyncNotificationClient? _overrideClient; 
        private static string? _lastSendType;
        private readonly IConfiguration _functionConfiguration;
        private readonly ICorrespondenceServiceValidator _correspondenceServiceValidator;

        public NotifyGateway(IServiceProvider serviceProvider)
        {      
                      
            _correspondenceServiceValidator = serviceProvider.GetService<ICorrespondenceServiceValidator>()!;
            _functionConfiguration = serviceProvider.GetService<IConfiguration>()!;
            _client = GetNotificationClient(_functionConfiguration!);

        }
             
        public NotifyGateway(IAsyncNotificationClient client, ICorrespondenceServiceValidator CorrespondenceServiceValidator, IConfiguration functionConfiguration)
        {
            _client = client ?? GetNotificationClient(_functionConfiguration!);
            _correspondenceServiceValidator = CorrespondenceServiceValidator;
            _functionConfiguration = functionConfiguration;
        }

        public NotifyGateway(IAsyncNotificationClient client, ICorrespondenceServiceValidator CorrespondenceServiceValidator, IConfiguration functionConfiguration, string lastSendType, IAsyncNotificationClient overrideClient)
        {
            _client = client ?? GetNotificationClient(functionConfiguration);
            _correspondenceServiceValidator = CorrespondenceServiceValidator;
            _functionConfiguration = functionConfiguration;
            _lastSendType = lastSendType;
            _overrideClient = overrideClient; 
        }

        public async Task<string> SendEmailAsync(EmailContent emailContent, byte[] fileAttachment, string specifiedSendType)
        {
         
            if (fileAttachment != null)
            {
                emailContent.Personalisation!.Add(emailContent.FilePersonalisationName!, NotificationClient.PrepareUpload(fileAttachment, emailContent.FileName));
            }

            var setClient = SetNotificationClientBasedOnSendType(specifiedSendType);

            var response = await setClient.SendEmailAsync(emailContent.EmailAddress, emailContent.TemplateId, emailContent.Personalisation, null, emailContent.EmailReplyToId);

            return response.id;
        }

        public async Task<string> SendLetterAsync(LetterContent letterContent, string specifiedSendType)
        {
              
            var setClient = SetNotificationClientBasedOnSendType(specifiedSendType);

            var response = await setClient.SendLetterAsync(letterContent.TemplateId, letterContent.Personalisation);

            return response.id;
        }

        public async Task<string> SendSmsAsync(SmsContent smsContent, string specifiedSendType)
        {
               
           var setClient = SetNotificationClientBasedOnSendType(specifiedSendType);

           var response = await setClient.SendSmsAsync(smsContent.MobileNumber, smsContent.TemplateId, smsContent.Personalisation);

            return response.id;
        }

        public async Task<string> SendPrecompiledLetterAsync(PrecompiledLetterContent precompiledletterContent, byte[] pdfContents, string specifiedSendType)
        {

            var setClient = SetNotificationClientBasedOnSendType(specifiedSendType);

            string postage = precompiledletterContent.Postage switch
            {
                PrecompiledLetterPostage a when a.Equals(PrecompiledLetterPostage.FirstClass) => "first",
                PrecompiledLetterPostage b when b.Equals(PrecompiledLetterPostage.SecondClass) => "second",
                _ => "second"
            };
            var response = await setClient.SendPrecompiledLetterAsync(precompiledletterContent.ClientReference.ToString(), pdfContents, postage);

            return response.id;
        }

        public async Task CheckStatusUpdatesAsync(IEnumerable<NotificationMessage> messages)
        {
            foreach (var message in messages)
            {
                if (string.IsNullOrEmpty(message.ExternalId))
                {
                    message.Status = NotificationStatus.Unknown;
                    continue;
                }
                try
                {
                    var response = await _client.GetNotificationByIdAsync(message.ExternalId);
                    UpdateMessage(message, response.status.ToLowerInvariant());
                }
                catch (Exception ex)
                {
                    UpdateMessageFromNotifyException(message, ex);
				}
            }
        }

		protected virtual void UpdateMessageFromNotifyException(NotificationMessage message, Exception ex)
		{
			var errorstatus = NotificationStatus.Unknown;

			if (ex.Source == "GovukNotify")
			{
				errorstatus = ex.Message switch
				{
					string a when a.Contains("Status code 400")
								|| a.Contains("Status code 404") => NotificationStatus.Unknown,
					string b when b.Contains("Status code 403") => NotificationStatus.Rejected,
					_ => NotificationStatus.Unknown
				};
			}
			message.Status = errorstatus;
            message.Errors = ex.Message;
		}

		protected virtual void UpdateMessage(NotificationMessage message, string responseStatus)
        {

            message.Status = responseStatus switch
            {
                string a when a.Contains("created") || a.Contains("pending-virus-check") =>  NotificationStatus.SendingToProvider,
                string b when b.Contains("sending") || b.Contains("pending") || b.Contains("accepted") => NotificationStatus.SendingToRecipient,
                string c when c.Contains("sent") || c.Contains("received") => NotificationStatus.DeliveredUnconfirmed,
                string d when d.Contains("delivered") => NotificationStatus.DeliveredConfirmed,
                string e when e.Contains("cancelled") => NotificationStatus.Cancelled,
                string f when f.Contains("temporary-failure") => NotificationStatus.Rejected,
                string g when g.Contains("permanent-failure") => NotificationStatus.UserError,
                string h when h.Contains("technical-failure") => NotificationStatus.SystemFailure,
                string i when i.Contains("validation-failed") => NotificationStatus.ValidationFailed,
                string j when j.Contains("virus-scan-failed") => NotificationStatus.VirusScanFailed,
                _ => NotificationStatus.Unknown
            };

            if (message.Status == NotificationStatus.Unknown)
            {
                message.Errors += $" GovNotify Response Status: {responseStatus}.";
            }
        }

		private static NotificationClient GetNotificationClient(IConfiguration config = null!, string specifiedSendType = null!)
        {
           
            var url = config["GovNotifyPassThroughURL"];
            var passthroughKey = config["GovNotifyPassThroughKey"];
            var passthroughVersion = config["GovNotifyPassThroughVersion"];
            var defaultGovNotifyAPIKey = config["GovNotifyApiKey_DefaultMethod"];

            var apiKey = specifiedSendType != null ? config[$"GovNotifyApiKey_{specifiedSendType}"] :
                                                     config[$"GovNotifyApiKey_{defaultGovNotifyAPIKey}"]; 
            var client = new HttpClient();
            client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", passthroughKey);
            client.DefaultRequestHeaders.Add("Accept-Version", passthroughVersion);
            var wrapper = new HttpClientWrapper(client);
            var notClient = new NotificationClient(wrapper, apiKey);
            // Override the base url to use pass through APIM
            wrapper.BaseAddress = new Uri(url!);
            return notClient;
        }

        private IAsyncNotificationClient SetNotificationClientBasedOnSendType(string specifiedSendType)
        {
            IAsyncNotificationClient setClient = null!;
            var config = _functionConfiguration;
            var defaultGovNotifyAPIKey = config["GovNotifyApiKey_DefaultMethod"];
            var defaultGovNotifyAPIKeyValue = config[$"GovNotifyApiKey_{defaultGovNotifyAPIKey}"];
            var specifiedAPIKeyValue = !string.IsNullOrWhiteSpace(specifiedSendType) ? config[$"GovNotifyApiKey_{specifiedSendType}"] : null;

            if (!string.IsNullOrWhiteSpace(specifiedSendType) && !specifiedAPIKeyValue!.Equals(defaultGovNotifyAPIKeyValue))
            {
                if (specifiedSendType != _lastSendType)
                {
                    _overrideClient = GetNotificationClient(_functionConfiguration, specifiedSendType);
                    setClient = _overrideClient;
                    _lastSendType = specifiedSendType;

                } else { setClient = _overrideClient!; }
            
            } else {
                setClient = _client;
             }

            return setClient;
        }


        public async Task<TemplatePreviewResponse> GenerateTemplatePreviewAsync(TemplatePreviewRequest previewRequest)
        {
            
            var validatedPreviewContent = _correspondenceServiceValidator.ValidateTemplatePreviewRequest(previewRequest);

            var templatePreviewContent = new TemplatePreviewContent
            {
                CreatedOn = DateTime.UtcNow,
                TemplateId = validatedPreviewContent["TemplateId"].ToString(),
                ReturnAsHtml = Convert.ToBoolean(validatedPreviewContent["ReturnAsHtml"]),
                Personalisation = validatedPreviewContent.TryGetValue("Personalisation", out var p)
                                ? p as Dictionary<string, object> ?? new Dictionary<string, object>()
                                : null
            };

            var response = new Notify.Models.Responses.TemplatePreviewResponse();
            Regex _breakRegex = new("(\r?\n)+");

            try
            {
                response = await _client.GenerateTemplatePreviewAsync(templatePreviewContent.TemplateId, templatePreviewContent.Personalisation);
            }
            catch (Exception ex) 
            {            
                var error = Error.GetError<NotifyGateway>();
                error.Description += ex.Message;

                throw new StatusCodeException(error, error.Description, null, StatusCodes.Status400BadRequest);
            }
            
            var responseBody = templatePreviewContent.ReturnAsHtml == true ? _breakRegex.Replace(response.body, "<br/>")
                                                                      : response.body;
            
            var templatePreviewResponse = new TemplatePreviewResponse(response.type,
                                                                      response.id,   
                                                                      response.version,                                                                     
                                                                      response.subject,
                                                                      responseBody);

            return templatePreviewResponse;
        }
 
    }
}
