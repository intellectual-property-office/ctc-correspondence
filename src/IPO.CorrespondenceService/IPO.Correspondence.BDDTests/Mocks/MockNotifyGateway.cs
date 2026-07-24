using IPO.Correspondence.Interfaces.Gateways;
using IPO.Correspondence.Models;

namespace IPO.Correspondence.BDDTests.Mocks
{
    public class MockNotifyGateway : INotifyGateway
    {
        public Task CheckStatusUpdatesAsync(IEnumerable<NotificationMessage> messages)
        {
            return Task.CompletedTask;
        }

        public Task<TemplatePreviewResponse> GenerateTemplatePreviewAsync(TemplatePreviewRequest previewRequest)
        {
            return Task.FromResult(new TemplatePreviewResponse("","",1,"",""));
        }

        public Task<string> SendEmailAsync(EmailContent emailContent, byte[] fileAttachment, string sendType)
        {
            return Task.FromResult(string.Empty);
        }

        public Task<string> SendLetterAsync(LetterContent letterContent, string sendType)
        {
            return Task.FromResult(string.Empty);
        }

        public Task<string> SendPrecompiledLetterAsync(PrecompiledLetterContent precompiledletterContent, byte[] pdfContents, string sendType)
        {
            return Task.FromResult(string.Empty);
        }

        public Task<string> SendSmsAsync(SmsContent smsContent, string sendType)
        {
            return Task.FromResult(string.Empty);
        }
    }
}
