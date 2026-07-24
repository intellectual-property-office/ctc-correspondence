using Swashbuckle.AspNetCore.Filters;
using System.Collections.Generic;

namespace IPO.Correspondence.Models
{
    public class TemplatePreviewResponseExamples : IMultipleExamplesProvider<TemplatePreviewResponse>
    {
        public IEnumerable<SwaggerExample<TemplatePreviewResponse>> GetExamples()
        {
            yield return SwaggerExample.Create("Email Template Preview", GetEmailTemplatePreviewRequest());
            yield return SwaggerExample.Create("Email Template Preview With Body using HTML tag </br>", GetEmailTemplatePreviewHTMLRequest());
            yield return SwaggerExample.Create("Letter Template Preview", GetLetterTemplatePreviewRequest());
            yield return SwaggerExample.Create("SMS Template Preview", GetSMSTemplatePreviewRequest());
        }
        private static TemplatePreviewResponse GetEmailTemplatePreviewRequest()
        {
            return new TemplatePreviewResponse("Email", "1", 1, "This is a test subject for a Email template", "Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua. Ut enim ad minim veniam, quis nostrud exercitation ullamco laboris nisi ut aliquip ex ea commodo consequat. Duis aute irure dolor in reprehenderit in voluptate velit esse cillum dolore eu fugiat nulla pariatur. ");
        }

        private static TemplatePreviewResponse GetEmailTemplatePreviewHTMLRequest()
        {
            return new TemplatePreviewResponse("Email", "2", 1, "This is a test subject for a Email template", "Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua.<br/>Ut enim ad minim veniam, quis nostrud exercitation ullamco laboris nisi ut aliquip ex ea commodo consequat.<br/>Duis aute irure dolor in reprehenderit in voluptate velit esse cillum dolore eu fugiat nulla pariatur.<br/>");
        }

        private static TemplatePreviewResponse GetLetterTemplatePreviewRequest()
        {
            return new TemplatePreviewResponse("Letter", "3", 1, "This is a test subject for a Letter template", "This is a body subject for Letter template");
        }

        private static TemplatePreviewResponse GetSMSTemplatePreviewRequest()
        {
            return new TemplatePreviewResponse("SMS", "4", 1, null!, "This is a test body for a SMS template");
        }
              
    }
}
