#pragma warning disable CS1998

using Newtonsoft.Json;
using System.IO;
using System.Threading.Tasks;

namespace ASF.Shared.SyncData
{
    public class LocalFileSystemSyncService : ISyncService
    {
        private JsonSerializer serializer = new JsonSerializer();
        private string directory;

        public LocalFileSystemSyncService(string directory)
        {
            this.directory = directory;
        }

        protected string GetFilePath(string shelterID, string flashcardSetID)
            => $"{directory.TrimEnd('\\')}\\shelter-{shelterID}-flashcard-set-{flashcardSetID}.json";

        public async Task<SyncMetadata> GetMetadata(string flashcardsID, string shelterID)
        {
            string filePath = GetFilePath(flashcardsID, shelterID);
            if (!File.Exists(filePath)) return null;

            using (Stream data = new FileStream(filePath, FileMode.Open))
            using (TextReader reader = new StreamReader(data))
            {
                return (SyncMetadata)serializer.Deserialize(reader, typeof(SyncMetadata));
            }
        }

        public async Task Save(SyncMetadata metadata)
        {
            string filePath = GetFilePath(metadata.ShelterID, metadata.FlashcardSetID);

            using (Stream data = new FileStream(filePath, FileMode.Create))
            using (TextWriter writer = new StreamWriter(data))
            {
                serializer.Serialize(writer, metadata, typeof(SyncMetadata));
            }
        }
    }
}
