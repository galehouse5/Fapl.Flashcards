using System;
using System.Linq;

namespace ASF.Shared.Cram
{
    public class CramCard : IFlashcard
    {
        public int? CardID { get; set; }
        public string Front { get; set; }
        public string Back { get; set; }
        public string Hint { get; set; }
        public string ImageFront { get; set; }
        public string ImageUrl { get; set; }
        public string ImageHint { get; set; }

        string IFlashcard.ID => CardID.ToString();

        void IFlashcard.SetDescription(IPetDescription description)
        {
            CramHtmlBuilder builder = new CramHtmlBuilder();
            using (builder.StartParagraphTag())
            using (builder.StartBoldTag())
            using (builder.StartLargeTag())
            {
                builder.AppendText(description.Name);
            }

            using (builder.StartParagraphTag())
            using (builder.StartBoldTag())
            using (builder.StartSmallTag())
            {
                builder.AppendText(string.Join(", ", description.Breeds));
            }

            using (builder.StartParagraphTag())
            using (builder.StartBoldTag())
            using (builder.StartExtraSmallTag())
            {
                builder.AppendText($"{description.Age}, {description.Sex}, {description.Size}");
                builder.LineBreakTag();
            }

            var allParagraphs = description.Description
                .Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries)
                .ToArray();
            int includedParagraphsLength = 0;
            var includedParagraphs = allParagraphs
                .TakeWhile(p =>
                {
                    if (includedParagraphsLength >= 200)
                        return false;

                    includedParagraphsLength += p.Length;
                    return true;
                }).ToArray();

            foreach (string paragraph in includedParagraphs)
            {
                using (builder.StartParagraphTag())
                using (builder.StartExtraSmallTag())
                {
                    builder.AppendText(paragraph);
                    builder.LineBreakTag();
                }
            }

            if (allParagraphs.Count() > includedParagraphs.Count())
            {
                using (builder.StartParagraphTag())
                {
                    builder.AppendText("…");
                }
            }

            Back = builder.ToString();
        }

        void IFlashcard.SetImage(string imageID)
        {
            ImageFront = imageID;
        }
    }
}
