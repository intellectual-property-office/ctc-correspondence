using Azure;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using IPO.Common.Infrastructure;
using IPO.Correspondence.Interfaces.Gateways;
using IPO.ServiceRequest.Models.Configuration;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;

namespace IPO.Correspondence.Gateways
{
    public class BlobStorageGateway : IFileStorageGateway
    {
        private readonly BlobServiceClient _blobServiceClient;
        private readonly Settings _settings;
        private readonly ILogger<BlobStorageGateway> _logger;

        public BlobStorageGateway(BlobServiceClient blobServiceClient,
                                 IOptionsMonitor<Settings> optionsMonitor,
                                 ILogger<BlobStorageGateway> logger)

        {
            _blobServiceClient = blobServiceClient;
            _settings = optionsMonitor.CurrentValue;
            _logger = logger;
        }

        public async Task<MemoryStream> GetFileStreamAsync(string fileId)
        {
            var containerClient = _blobServiceClient.GetBlobContainerClient(_settings.ContainerName);
            var blobClient = containerClient.GetBlobClient(fileId);

            var memoryStream = new MemoryStream();
            _ = await blobClient.DownloadToAsync(memoryStream);

            memoryStream.Seek(0, SeekOrigin.Begin);
            return memoryStream;
        }

        public async Task<string> UploadAttachmentAsync(Stream file)
        {
            var containerClient = _blobServiceClient.GetBlobContainerClient(_settings.ContainerName);
            var fileId = Guid.NewGuid().ToString(); 
            var blobClient = containerClient.GetBlobClient(fileId);

            try
            {
                _ = await blobClient.UploadAsync(file, overwrite: true);

                return fileId;
            }
            catch (RequestFailedException ex)
            {
                var error = Error.GetError<BlobStorageGateway>();
                error.Description += $" Azure returned: {ex.ErrorCode}";
                throw new StatusCodeException(error, ex.Message, ex, ex.Status);
            }
        }

        public async Task DeleteAttachmentAsync(string fileId)
        {
            if (string.IsNullOrEmpty(fileId))
            {
                // Log the issue for diagnostics
                _logger.LogWarning("Attempted to delete a blob with a null or empty name.");
                return;
            }

            var containerClient = _blobServiceClient.GetBlobContainerClient(_settings.ContainerName);
            var blobClient = containerClient.GetBlobClient(fileId);
            try
            {
                _ = await blobClient.DeleteIfExistsAsync(DeleteSnapshotsOption.IncludeSnapshots);
            }
            catch (RequestFailedException rex)
            {
                var error = Error.GetError<BlobStorageGateway>();
                error.Description += $" Azure returned: {rex.ErrorCode}";
                throw new StatusCodeException(error, rex.Message, rex, rex.Status);
            }
        }

        public async Task DeleteAttachmentsAsync(IEnumerable<string> fileIds)
        {
            foreach (var fileId in fileIds)
            {
                await DeleteAttachmentAsync(fileId);
            }
        }

    }
}