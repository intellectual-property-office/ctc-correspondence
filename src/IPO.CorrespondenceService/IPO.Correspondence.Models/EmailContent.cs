using System.Collections.Generic;

namespace IPO.Correspondence.Models
{
    public class EmailContent
    {
        public string? FileId { get; set; }
        public string? FileName { get; set; }
        public string? FilePersonalisationName { get; set; }
        public string? EmailAddress { get; set; }
        public string? TemplateId { get; set; }
        public string? EmailReplyToId { get; set; }
        public Dictionary<string, object>? Personalisation { get; set; }
    }
}
