using System;
using System.Net;
using System.Text;

namespace FriendshipApl.Flashcards.Shared.Cram
{
    public class CramHtmlBuilder
    {
        private class TagCloser : IDisposable
        {
            private StringBuilder html;
            private string closeTag;

            public TagCloser(StringBuilder html, string closeTag)
            {
                this.html = html;
                this.closeTag = closeTag;
            }

            public void Dispose()
            {
                html.Append(closeTag);
            }
        }

        private StringBuilder html = new StringBuilder();

        protected IDisposable AppendUnclosedTag(string openTag, string closeTag)
        {
            html.Append(openTag);
            return new TagCloser(html, closeTag);
        }

        public IDisposable StartBoldTag()
            => AppendUnclosedTag("<b>", "</b>");

        public IDisposable StartParagraphTag()
            => AppendUnclosedTag("<p>", "</p>");

        public void LineBreakTag()
        {
            html.Append("<br />");
        }

        public IDisposable StartExtraSmallTag()
            => AppendUnclosedTag("<font class=\"font-xs\">", "</font>");

        public IDisposable StartSmallTag()
            => AppendUnclosedTag("<font class=\"font-s\">", "</font>");

        public IDisposable StartMediumTag()
            => AppendUnclosedTag("<font class=\"font-m\">", "</font>");

        public IDisposable StartLargeTag()
            => AppendUnclosedTag("<font class=\"font-l\">", "</font>");

        public void AppendText(string value)
        {
            html.Append(WebUtility.HtmlEncode(value));
        }

        public override string ToString()
            => html.ToString();
    }
}
