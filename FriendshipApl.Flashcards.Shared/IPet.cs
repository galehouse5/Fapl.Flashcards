using System;

namespace FriendshipApl.Flashcards.Shared
{
    public interface IPet : IPetDescription, IPetImages
    {
        string ID { get; }
        DateTime LastUpdateTimestamp { get; }
    }
}
