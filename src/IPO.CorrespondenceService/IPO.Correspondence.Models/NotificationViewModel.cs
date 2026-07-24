using System;
using Swashbuckle.AspNetCore.Annotations;

namespace IPO.Correspondence.Models
{
    [SwaggerSchema(Description = "This is the NotificationViewModel object used to represent Notification requests that were made to GOV.UK Notify")]
    public class NotificationViewModel
    {
        public NotificationViewModel()
        { }

        public NotificationViewModel(NotificationMessage message)
        {
            Id = message.Id.ToString();
            CreatedOn = message.CreatedOn;
            PayLoad = message.PayLoad!;
            MessageType = message.Type.ToString();
            ExternalId = message.ExternalId!;
            Status = message.Status;
            OwnerId = message.Owner?.Id;
            Errors = message.Errors!;   
        }

        public NotificationViewModel(NotificationMessage message, Guid ownerId): this(message)
        {
            OwnerId = ownerId;
        }

        [SwaggerSchema(Title = "Correspondence Microservice Notification Id")]
        public string? Id { get; set; }

        [SwaggerSchema(Title = "Date the Notification was created")]
        public DateTime CreatedOn { get; set; }

        [SwaggerSchema(Title = "Email/Letter/SMS/API")]
        public string? MessageType { get; set; }

        [SwaggerSchema(Title = "GOV.UK Notify Notification Id")]
        public string? ExternalId { get; set; }

        [SwaggerSchema(Title = "Status of the Notification")]
        public NotificationStatus? Status { get; private set; }

        [SwaggerSchema(Title = "Message Data contained in the Notification")]
        public string? PayLoad { get; set; }

        [SwaggerSchema(Title = "Id used to identify unread emails related to a specific business")]
        public Guid? OwnerId { get; private set; }
        
        [SwaggerSchema(Title = "Error message if notification Post Request failed. If Errors is null then notification Post request was OK")]
        public string? Errors { get; set; }
    }
}
