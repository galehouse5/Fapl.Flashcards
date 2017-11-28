using System.IO;

namespace FriendshipApl.Flashcards.Shared.Helpers
{
    public static class StreamHelper
    {
        public static byte[] ReadAll(this Stream value)
        {
            using (MemoryStream copy = new MemoryStream())
            {
                value.CopyTo(copy);
                copy.Position = 0;
                return copy.ToArray();
            }
        }
    }
}
