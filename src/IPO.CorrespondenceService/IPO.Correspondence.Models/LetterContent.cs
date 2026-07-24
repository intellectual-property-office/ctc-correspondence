using System.Collections.Generic;

namespace IPO.Correspondence.Models
{
    public class LetterContent
    {
        public string? TemplateId { get; set; }
        public Dictionary<string, object>? Personalisation { get; set; }
    }
}
