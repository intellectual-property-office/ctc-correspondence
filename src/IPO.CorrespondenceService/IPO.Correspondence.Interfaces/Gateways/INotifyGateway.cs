using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using IPO.Correspondence.Models;

namespace IPO.Correspondence.Interfaces.Gateways
{
    public interface INotifyGateway
    {
        Task<string> SendEmailAsync(EmailContent emailContent, byte[] fileAttachment, string sendType);
        Task<string> SendLetterAsync(LetterContent letterContent, string sendType);
        Task<string> SendPrecompiledLetterAsync(PrecompiledLetterContent precompiledletterContent, byte[] pdfContents, string sendType);
        Task<string> SendSmsAsync(SmsContent smsContent, string sendType);
        Task CheckStatusUpdatesAsync(IEnumerable<NotificationMessage> messages);
        Task<TemplatePreviewResponse> GenerateTemplatePreviewAsync(TemplatePreviewRequest previewRequest);
        
    }
}
