using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace ASF.Shared.ImageLayout
{
    public static class HorizontalRowImageLayout
    {
        public static IReadOnlyCollection<RectangleF> Generate(IReadOnlyCollection<SizeF> sizes)
        {
            float maxHeight = sizes.Max(s => s.Height);
            var layout = new List<RectangleF>();
            float x = 0f;

            foreach (SizeF size in sizes)
            {
                layout.Add(new RectangleF
                {
                    X = x,
                    Y = (maxHeight - size.Height) / 2f,
                    Width = size.Width,
                    Height = size.Height
                });

                x += size.Width;
            }

            return layout;
        }
    }
}
