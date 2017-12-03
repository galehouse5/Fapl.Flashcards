using System.Threading.Tasks;

namespace Fapl.Flashcards.Shared
{
    public interface ISyncService
    {
        Task<SyncMetadata> GetMetadata(string flashcardSetID, string shelterID);
        Task Save(SyncMetadata metadata);
    }
}
