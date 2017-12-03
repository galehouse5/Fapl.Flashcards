using System.Threading.Tasks;

namespace Fapl.Flashcards.Shared
{
    public interface IFlashcardService
    {
        Task<IFlashcardSet> GetSet(string id);
        Task Save(IFlashcardSet set);
        Task Add(IFlashcard card, IFlashcardSet set);
        Task Remove(IFlashcard card, IFlashcardSet set);
        Task Save(IFlashcard card, IFlashcardSet set);
        Task<string> UploadImage(string contentType, byte[] data);
    }
}
