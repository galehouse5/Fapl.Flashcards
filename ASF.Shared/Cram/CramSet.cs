using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace ASF.Shared.Cram
{
    public class CramSet : IFlashcardSet
    {
        public int SetID { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string Subject { get; set; }
        public string Access { get; set; }
        public string LangFront { get; set; }
        public string LangBack { get; set; }
        public string LangHint { get; set; }
        public List<CramCard> Cards { get; set; } = new List<CramCard>();

        string IFlashcardSet.ID => SetID.ToString();

        string IFlashcardSet.Title
        {
            get { return Title; }
            set { Title = value; }
        }

        IReadOnlyCollection<string> IFlashcardSet.Tags
        {
            get { return Subject.Split(',').Select(s => s.Trim()).ToArray(); }
            set { Subject = value == null ? null : string.Join(", ", value); }
        }

        string IFlashcardSet.Description
        {
            get { return Description; }
            set { Description = value; }
        }

        int IReadOnlyCollection<IFlashcard>.Count => Cards.Count;
        IFlashcard IFlashcardSet.CreateFlashcard() => new CramCard();
        IEnumerator<IFlashcard> IEnumerable<IFlashcard>.GetEnumerator() => Cards.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => Cards.GetEnumerator();
    }
}
