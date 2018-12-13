using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using SmartRoomsApp.API.Data;

namespace SmartRoomsApp.API.Helpers
{
    public class CloudStorageRepository : ICloudStorageRepository
    {
        private readonly CloudStorageAccount _account;
        private readonly CloudBlobClient _serviceClient;

        public CloudStorageRepository()
        {
            _account = CloudStorageAccount.DevelopmentStorageAccount;
            _serviceClient = _account.CreateCloudBlobClient();
        }

        public async Task<Stream> downloadStreamFromBlobContainer(string fileName, string containerName)
        {
            Stream stream = new MemoryStream();
            CloudBlockBlob blockBlob = await _getBlockBlob(containerName, fileName);
            await blockBlob.DownloadToStreamAsync(stream);
            stream.Position = 0;
            return stream;
        }

        public async Task<string> downloadTextFromBlobContainer(string fileName, string containerName)
        {
            CloudBlockBlob blockBlob = await _getBlockBlob(containerName, fileName);
            return await blockBlob.DownloadTextAsync();
        }

        public async Task<string> uploadTextToBlobContainer(string fileName, string text, string containerName)
        {
            CloudBlockBlob blockBlob = await _getBlockBlob(containerName, fileName);
            await blockBlob.UploadTextAsync(text);
            return blockBlob.Name;
        }

        private async Task<CloudBlockBlob> _getBlockBlob(string containerName, string fileName)
        {
            var container = _serviceClient.GetContainerReference(containerName);
            await container.SetPermissionsAsync(new BlobContainerPermissions { PublicAccess = BlobContainerPublicAccessType.Container });
            await container.CreateIfNotExistsAsync();
            return container.GetBlockBlobReference(fileName);
        }
    }
}