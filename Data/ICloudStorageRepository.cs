using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage.Blob;

namespace SmartRoomsApp.API.Data
{
    public interface ICloudStorageRepository
    {
        Task<string> downloadTextFromBlobContainer(string containerName, string fileName);
        Task<string> uploadTextToBlobContainer(string containerName, string fileName, string text);
    }
}