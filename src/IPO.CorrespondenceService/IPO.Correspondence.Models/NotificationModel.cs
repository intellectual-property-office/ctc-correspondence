using IPO.Correspondence.Models;
using Swashbuckle.AspNetCore.Annotations;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace IPO.Correspondence.Models
{
    [SwaggerSchema(Description = "Properties used for sending message requests to Gov.UK Notify")]
    public class NotificationModel
    {
        [Required]
        [SwaggerSchema(Title = "Type of message. Email/Letter/SMS/API")]        
        public NotificationType? Channel { get; set; }
              
        [SwaggerSchema(Title = "The user specified API Key. PretendToSend/SendToListOnly/SendToAnyone")]
        public GovNotifyAPIKeyType? SendType { get; set; }
         
        [Required]
        [SwaggerSchema(Title = "The content of the message request body: EmailAddress/TemplateId/Personalisation data etc.")]
        public Dictionary<string, object>? Content { get; set; }
    }
}
