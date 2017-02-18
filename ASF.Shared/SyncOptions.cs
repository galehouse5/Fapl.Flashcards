using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;

namespace ASF.Shared
{
    public class SyncOptions
    {
        public IReadOnlyCollection<PetStatus> PetStatuses { get; set; }
            = new[] { PetStatus.Adoptable };
        public Func<IPet, bool> PetFilter { get; set; } = p => true;
        public string FlashcardSetTitle { get; set; }
        public IReadOnlyCollection<string> FlashcardSetTags { get; set; }
        public string FlashcardSetDescription { get; set; }
        public float FlashcardImageMargin { get; set; } = 5f;
        public Color? FlashcardImageBackgroundColor { get; set; } = Color.White;
        public ImageFormat FlashcardImageFormat { get; set; } = ImageFormat.Jpeg;
    }
}
