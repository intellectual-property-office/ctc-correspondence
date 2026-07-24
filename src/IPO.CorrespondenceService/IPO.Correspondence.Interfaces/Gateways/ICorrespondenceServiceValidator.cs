using System.Collections.Generic;
using System.Threading.Tasks;
using IPO.Correspondence.Models;


namespace IPO.Correspondence.Interfaces.Gateways
{
    public interface ICorrespondenceServiceValidator
    {
        public Dictionary<string, object> ValidateTemplatePreviewRequest(TemplatePreviewRequest previewRequest);
        public void VerifySpecifiedSendType(GovNotifyAPIKeyType sendType);
    }
}
