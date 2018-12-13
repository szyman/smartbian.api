using System.IO;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage.Blob;

namespace SmartRoomsApp.API.Data
{
    public interface ICloudStorageRepository
    {
        Task<Stream> downloadStreamFromBlobContainer(string fileName, string containerName = "smartbiancontainer");
        Task<string> downloadTextFromBlobContainer(string fileName, string containerName = "smartbiancontainer");
        Task<string> uploadTextToBlobContainer(string fileName, string text, string containerName = "smartbiancontainer");
    }
}