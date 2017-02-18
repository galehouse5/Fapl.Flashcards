using System;

namespace ASF.Shared
{
    public interface IPet : IPetDescription, IPetImages
    {
        string ID { get; }
        DateTime LastUpdateTimestamp { get; }
    }
}
