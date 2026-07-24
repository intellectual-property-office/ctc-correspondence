using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace IPO.Correspondence.Models
{
    public class NotificationMessage
    {
        public Guid Id { get; set; }
        public NotificationType Type { get; set; }
        public DateTime CreatedOn { get; set; }
        public NotificationOwner? Owner { get; set; }
        public NotificationStatus Status { get; set; }        
        [NotMapped]
        public GovNotifyAPIKeyType? APIKeyType { get; set; }
        public string? PayLoad { get; set; }
        public string? ExternalId { get; set; }
        public bool InternalRead { get; set; }
        public bool ExternalRead { get; set; }
        public Guid OrganisationId { get; set; }
        public string? Errors { get; set; }
    }
}
