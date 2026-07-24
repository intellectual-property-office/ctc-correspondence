using System.Collections.Generic;

namespace IPO.Correspondence.Models
{
    public class SmsContent
    {
        public string? MobileNumber { get; set; }
        public string? TemplateId { get; set; }
        public Dictionary<string, object>? Personalisation { get; set; }
    }
}
