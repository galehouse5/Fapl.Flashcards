using System.Collections.Generic;

namespace FriendshipApl.Flashcards.Shared
{
    public interface IFlashcardSet : IReadOnlyCollection<IFlashcard>
    {
        string ID { get; }
        string Title { get; set; }
        IReadOnlyCollection<string> Tags { get; set; }
        string Description { get; set; }

        IFlashcard CreateFlashcard();
    }
}
