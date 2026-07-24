using System.Collections.Generic;

namespace IPO.Correspondence.Models
{
    public class ApiContent
    {
        public string? Email { get; set; }
        public string? TemplateId { get; set; }
        public Dictionary<string, object>? Personalisation { get; set; }
    }
}
