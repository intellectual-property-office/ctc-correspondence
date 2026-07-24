using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace IPO.Correspondence.Interfaces.Gateways
{
    public interface IFileStorageGateway
    {
        public Task<string> UploadAttachmentAsync(Stream file);

        Task<MemoryStream> GetFileStreamAsync(string fileId);

        Task DeleteAttachmentAsync(string fileId);

        Task DeleteAttachmentsAsync(IEnumerable<string> fileIds);
    }
}