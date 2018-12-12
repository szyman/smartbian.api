using System;
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

        public async Task<string> downloadTextFromBlobContainer(string containerName, string fileName)
        {

                CloudBlockBlob blockBlob = await _getBlockBlob(containerName, fileName);
                return await blockBlob.DownloadTextAsync();

        }

        public async Task<string> uploadTextToBlobContainer(string containerName, string fileName, string text)
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