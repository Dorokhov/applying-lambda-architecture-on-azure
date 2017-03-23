using System;
using System.Globalization;
using System.Threading.Tasks;
using DataContracts;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using Newtonsoft.Json;
using ResourceConfiguration;

namespace BatchLayer
{
    public class MeetupRepository
    {
        private readonly CloudBlobContainer _container;

        public MeetupRepository(StorageConfiguration storageConfiguration)
        {
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(storageConfiguration.ConnectionString);
            CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();
            _container = blobClient.GetContainerReference(storageConfiguration.MasterDataSetContainerName);
        }
        
        public async Task SaveAsync(MeetupMessage message)
        {
            CloudBlockBlob blockBlob = _container.GetBlockBlobReference($"meetup_{DateTime.UtcNow.ToString("yyyyMMddHHmmss")}_{Guid.NewGuid()}");
            await blockBlob.UploadTextAsync(JsonConvert.SerializeObject(message));
        }
    }
}
