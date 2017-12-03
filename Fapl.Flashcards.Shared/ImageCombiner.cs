using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace Fapl.Flashcards.Shared
{
    public delegate IReadOnlyCollection<RectangleF> GenerateImageLayout(IReadOnlyCollection<SizeF> sizes);

    public static class ImageCombiner
    {
        public static Image CombineImages(this IReadOnlyCollection<Image> images,
            GenerateImageLayout layoutGenerator, float margin = 0f, Color? backgroundColor = null)
        {
            if (!images.Any())
                throw new ArgumentException("Must supply at least one image.", nameof(images));

            var layout = layoutGenerator(images
                .Select(i => i.PhysicalDimension)
                .Select(s => new SizeF(width: s.Width + margin, height: s.Height + margin))
                .ToArray());
            SizeF boundingBox = new SizeF(
                width: layout.Max(b => b.Right) + margin,
                height: layout.Max(b => b.Bottom) + margin);
            Bitmap combined = new Bitmap(
                width: (int)Math.Ceiling(boundingBox.Width ),
                height: (int)Math.Ceiling(boundingBox.Height));

            try
            {
                using (Graphics graphics = Graphics.FromImage(combined))
                {
                    if (backgroundColor.HasValue)
                    {
                        graphics.Clear(backgroundColor.Value);
                    }

                    foreach (var pair in layout.Zip(images,
                        (boundary, image) => new { boundary, image }))
                    {
                        graphics.DrawImage(pair.image,
                            x: pair.boundary.X + margin,
                            y: pair.boundary.Y + margin,
                            width: pair.boundary.Width - margin,
                            height: pair.boundary.Height - margin);
                    }
                }

                return combined;
            }
            catch (Exception)
            {
                combined.Dispose();
                throw;
            }
        }
    }
}
