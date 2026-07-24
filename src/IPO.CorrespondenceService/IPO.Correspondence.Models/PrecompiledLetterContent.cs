using System;

namespace IPO.Correspondence.Models
{
    public class PrecompiledLetterContent
    {
        public string? FileId { get; set; }
        public string? FileName { get; set; }
        public Guid ClientReference { get; set; }
        public PrecompiledLetterPostage? Postage { get; set; }
    }
}
