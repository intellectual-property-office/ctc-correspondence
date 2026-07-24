using IPO.Correspondence.Interfaces.Gateways;
using IPO.Correspondence.Models;
using System;
using System.Collections.Generic;
using IPO.Common.Infrastructure;
using Microsoft.AspNetCore.Http;
using System.Linq;
using Microsoft.Extensions.Configuration;

namespace IPO.Correspondence.Services.Validation
{
    public class CorrespondenceServiceValidator : ICorrespondenceServiceValidator
    {
        private readonly IConfiguration _configuration;

        public CorrespondenceServiceValidator(IConfiguration configuration = null!)
        {
            _configuration = configuration;

        }

        public Dictionary<string, object> ValidateTemplatePreviewRequest(TemplatePreviewRequest previewRequest)
        {

            var previewContent = new Dictionary<string, object>(previewRequest.Content!, StringComparer.OrdinalIgnoreCase);

            if (!previewContent.ContainsKey(TemplatePreviewContentParameters.templateId.ToString()) ||
                (previewContent[TemplatePreviewContentParameters.templateId.ToString()].Equals(String.Empty)))
            {

                var invalidParameter = TemplatePreviewContentParameters.templateId.ToString();

                var error = Error.GetError<CorrespondenceServiceValidator>();
                error.Description += " Missing mandatory request body parameter and/or value: " + invalidParameter + ".";
                throw new StatusCodeException(error, error.Description, null, StatusCodes.Status422UnprocessableEntity);

            };

            if (!previewContent.ContainsKey(TemplatePreviewContentParameters.returnAsHtml.ToString()))
            {
                previewContent.Add(TemplatePreviewContentParameters.returnAsHtml.ToString(), false);
            }
            else if (previewContent.ContainsKey(TemplatePreviewContentParameters.returnAsHtml.ToString()))
            {
                var returnAsHtmlvalue = previewContent[TemplatePreviewContentParameters.returnAsHtml.ToString()];

                if (!returnAsHtmlvalue.Equals(true) && (!returnAsHtmlvalue.Equals(false)))
                {
                    var error = Error.GetError<CorrespondenceServiceValidator>();
                    error.Description += " Invalid 'returnAsHtml' value. Please set value to 'true' or 'false'.";

                    throw new StatusCodeException(error, error.Description, null, StatusCodes.Status422UnprocessableEntity);
                }
            }

            if (!previewContent.ContainsKey(TemplatePreviewContentParameters.personalisation.ToString()))
            {
                previewContent.Add(TemplatePreviewContentParameters.personalisation.ToString(), new { });
            }

            return previewContent;
        }

        public void VerifySpecifiedSendType(GovNotifyAPIKeyType sendType)
        {
            string allowedMethods = _configuration["GovNotifyApiKey_AllowedMethods"]!;

            List<string> allowedMethodsList = allowedMethods.Split(',').Select(item => item.Trim())
                                                                                           .ToList();

            if (!allowedMethodsList.Contains(sendType.ToString()))
            {
                var error = Error.GetError<CorrespondenceServiceValidator>();
                error.Description += $" sendType: {sendType} is not available for this environment.";
                throw new StatusCodeException(error, error.Description, null, StatusCodes.Status422UnprocessableEntity);
            }
        }
             
    }
}
