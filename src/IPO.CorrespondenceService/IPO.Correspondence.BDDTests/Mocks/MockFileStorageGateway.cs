using IPO.Correspondence.Interfaces.Gateways;

namespace IPO.Correspondence.BDDTests.Mocks
{
    public class MockFileStorageGateway : IFileStorageGateway
    {
        public Task<MemoryStream> GetFileStreamAsync(string fileId)
        {
            return Task.FromResult(new MemoryStream());
        }

        public Task<string> UploadAttachmentAsync(Stream file)
        {
            return Task.FromResult(Guid.NewGuid().ToString());
        }

        public Task DeleteAttachmentAsync(string fileId)
        {
            return Task.CompletedTask;
        }

        public Task DeleteAttachmentsAsync(IEnumerable<string> fileIds)
        {
            return Task.CompletedTask;
        }
    }
}
