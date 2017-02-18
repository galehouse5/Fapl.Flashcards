using System.Threading.Tasks;

namespace ASF.Shared
{
    public interface ISyncService
    {
        Task<SyncMetadata> GetMetadata(string flashcardSetID, string shelterID);
        Task Save(SyncMetadata metadata);
    }
}
