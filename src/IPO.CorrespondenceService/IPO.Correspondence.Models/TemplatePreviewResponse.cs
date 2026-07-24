using Swashbuckle.AspNetCore.Annotations;

namespace IPO.Correspondence.Models
{
    [SwaggerSchema(Description = "This is the response body for the Preview endpoint")]
    public class TemplatePreviewResponse
    {
        public TemplatePreviewResponse(string type, string id, int version, string subject, string body)
        {
            Type = type;
            Id = id;       
            Version = version;
            Subject = subject;
            Body = body;           
        }

        [SwaggerSchema(Title = "The preview template's notification type. E.g. Email/SMS/Letter")]
        public string Type { get; set; }

        [SwaggerSchema(Title = "The Gov.UK Notify template Id")]
        public string Id { get; set; }
        
        [SwaggerSchema(Title = "The version number of the preview template")]
        public int Version { get; set; }

        [SwaggerSchema(Title = "The preview template's subject title. Note: This will always be null if type is SMS")]
        public string Subject { get; set; }

        [SwaggerSchema(Title = "The preview template's message body")]
        public string Body { get; set; }

   
    }
}
