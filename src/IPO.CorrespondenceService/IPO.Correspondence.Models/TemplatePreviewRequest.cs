using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Swashbuckle.AspNetCore.Annotations;

namespace IPO.Correspondence.Models
{
    [SwaggerSchema(Description = "This is the request body for the Preview endpoint")]
    public class TemplatePreviewRequest
    {
        [Required]
        [SwaggerSchema(Title = "The required preview request data: TemplateId/ReturnAsHtml/Personalisation and its data")]
        public Dictionary<string, object>? Content { get; set; }
    }
}
