using Swashbuckle.AspNetCore.Annotations;
using System;
using System.Collections.Generic;

namespace IPO.Correspondence.Models
{
    public enum TemplatePreviewContentParameters
    {
        templateId,
        returnAsHtml,
        personalisation
    }

    [SwaggerSchema(Description = "This is the TemplatePreviewContent object created as part of request for the Preview endpoint")]
    public class TemplatePreviewContent
    {

        [SwaggerSchema(Title = "Date and time of request creation")]
        public DateTime CreatedOn { get; set; }

        [SwaggerSchema(Title = "The Gov.UK Notify template Id")]
        public string? TemplateId { get; set; }

        [SwaggerSchema(Title = "Returns all line breaks in the body with </br>")]
        public bool ReturnAsHtml { get; set; }

        [SwaggerSchema(Title = "The GovNotify template placeholder names and their respective data")]
        public Dictionary<string, object>? Personalisation { get; set; }
    }
}
