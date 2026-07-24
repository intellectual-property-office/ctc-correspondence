using Swashbuckle.AspNetCore.Filters;
using System.Collections.Generic;

namespace IPO.Correspondence.Models
{
    public class TemplatePreviewRequestExamples : IMultipleExamplesProvider<TemplatePreviewRequest>
    {
        public IEnumerable<SwaggerExample<TemplatePreviewRequest>> GetExamples()
        {
            yield return SwaggerExample.Create("Template Preview Body Request (No Template Personalisation Data Example)", GetTemplatePreviewRequestBody());
            yield return SwaggerExample.Create("With HTML Line Break tags", GetTemplatePreviewRequestBodyWithHTMLLineBreak());
            yield return SwaggerExample.Create("With Template Personalisation Data", GetTemplatePreviewRequestBodyWithPersonalisation());
            yield return SwaggerExample.Create("With All Parameters", GetTemplatePreviewRequestBodyAllParameters());
        }
        private static TemplatePreviewRequest GetTemplatePreviewRequestBody()
        {
            return new TemplatePreviewRequest
            {

                Content = new Dictionary<string, object>()
                {
                          {"templateId", "00000000-0000-0000-0000-000000000000"}
                }
            };
        }

        private static TemplatePreviewRequest GetTemplatePreviewRequestBodyWithHTMLLineBreak()
        {
            return new TemplatePreviewRequest
            {
                Content = new Dictionary<string, object>()
                {
                          {"templateId", "00000000-0000-0000-0000-000000000000"},
                          {"returnAsHtml", false}
                }
            };
        }
                private static TemplatePreviewRequest GetTemplatePreviewRequestBodyWithPersonalisation()
        {
            return new TemplatePreviewRequest
            {

                Content = new Dictionary<string, object>()
                {
                          {"templateId", "00000000-0000-0000-0000-000000000000"},
                          {"personalisation", new {
                                                   TemplatePlaceholderFieldNameA = "DataA",
                                                   TemplatePlaceholderFieldNameB = "DataB"
                                                  }
                          }
                }
            };
        }

        private static TemplatePreviewRequest GetTemplatePreviewRequestBodyAllParameters()
        {
            return new TemplatePreviewRequest
            {
                Content = new Dictionary<string, object>()
                {
                          {"templateId", "00000000-0000-0000-0000-000000000000"},
                          {"returnAsHtml", false},
                          {"personalisation", new {
                                                   TemplatePlaceholderFieldNameA = "DataA",
                                                   TemplatePlaceholderFieldNameB = "DataB"
                                                  }
                          }
                }
            };
        }

    }
}
