using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using Newtonsoft.Json;
using System.IO;
using System.Threading.Tasks;

namespace ASF.Shared.SyncData
{
    public class AzureBlobSyncService : ISyncService
    {
        private JsonSerializer serializer = new JsonSerializer();
        private CloudBlobClient client;

        public AzureBlobSyncService(string connectionString)
        {
            CloudStorageAccount account = CloudStorageAccount.Parse(connectionString);
            client = account.CreateCloudBlobClient();
        }

        protected async Task<CloudBlockBlob> GetBlobReference(string shelterID, string flashcardSetID)
        {
            CloudBlobContainer container = client.GetContainerReference("sync-metadata");
            await container.CreateIfNotExistsAsync();

            return container.GetBlockBlobReference($"shelter-{shelterID}-flashcard-set-{flashcardSetID}.json");
        }

        public async Task<SyncMetadata> GetMetadata(string flashcardsID, string shelterID)
        {
            var blobReference = await GetBlobReference(shelterID, flashcardsID);
            if (!await blobReference.ExistsAsync()) return null;

            using (Stream data = new MemoryStream())
            using (TextReader reader = new StreamReader(data))
            {
                await blobReference.DownloadToStreamAsync(data);
                data.Position = 0;

                return (SyncMetadata)serializer.Deserialize(reader, typeof(SyncMetadata));
            }
        }

        public async Task Save(SyncMetadata metadata)
        {
            var blobReference = await GetBlobReference(metadata.ShelterID, metadata.FlashcardSetID);

            using (Stream data = new MemoryStream())
            using (TextWriter writer = new StreamWriter(data))
            {
                serializer.Serialize(writer, metadata, typeof(SyncMetadata));
                writer.Flush();

                data.Position = 0;
                await blobReference.UploadFromStreamAsync(data);
            }
        }
    }
}