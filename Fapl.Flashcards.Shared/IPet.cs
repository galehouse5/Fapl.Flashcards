using System;

namespace Fapl.Flashcards.Shared
{
    public interface IPet : IPetDescription, IPetImages
    {
        string ID { get; }
        DateTime LastUpdateTimestamp { get; }
    }
}
