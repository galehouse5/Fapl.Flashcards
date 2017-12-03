using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace Fapl.Flashcards.Shared.ImageLayout
{
    public static class RecursiveShrinkingImageLayout
    {
        public class MutableRectangleF
        {
            public PointF Location { get; set; }
            public SizeF Size { get; set; }

            public RectangleF ToRectangleF()
                => new RectangleF(Location, Size);

            public override string ToString()
                => $"X = {Location.X} Y = {Location.Y} Width = {Size.Width} Height = {Size.Height}";
        }

        public enum Orientation { Horizontal, Vertical }

        public static Orientation Oppose(this Orientation orientation)
        {
            if (orientation == Orientation.Horizontal) return Orientation.Vertical;
            if (orientation == Orientation.Vertical) return Orientation.Horizontal;
            throw new NotSupportedException();
        }

        public static float GetLength(this Orientation orientation, SizeF size)
        {
            if (orientation == Orientation.Horizontal) return size.Width;
            if (orientation == Orientation.Vertical) return size.Height;
            throw new NotSupportedException();
        }

        public static float GetProportion(this Orientation orientation, SizeF size)
        {
            if (orientation == Orientation.Horizontal) return size.Height / size.Width;
            if (orientation == Orientation.Vertical) return size.Width / size.Height;
            throw new NotSupportedException();
        }

        public static PointF Displace(this PointF location, SizeF displacement, Orientation orientation)
            => new PointF
            {
                X = orientation == Orientation.Horizontal ? location.X + displacement.Width : location.X,
                Y = orientation == Orientation.Vertical ? location.Y + displacement.Height : location.Y
            };

        public static SizeF Scale(this SizeF size, float factor)
            => new SizeF { Width = size.Width * factor, Height = size.Height * factor };

        public static PointF Scale(this PointF location, float factor)
            => new PointF { X = location.X * factor, Y = location.Y * factor };

        public static void LayOut(this IEnumerable<MutableRectangleF> elements, Orientation orientation)
        {
            Orientation opposingOrientation = orientation.Oppose();
            elements = elements.OrderBy(e => opposingOrientation.GetProportion(e.Size));

            if (elements.Count() == 2)
            {
                MutableRectangleF firstElement = elements.First();
                MutableRectangleF secondElement = elements.Skip(1).First();

                float firstOpposingLength = opposingOrientation.GetLength(firstElement.Size);
                float secondOpposingLength = opposingOrientation.GetLength(secondElement.Size);
                float minOppostingLength = Math.Min(firstOpposingLength, secondOpposingLength);

                firstElement.Location = PointF.Empty;
                firstElement.Size = firstElement.Size.Scale(minOppostingLength / firstOpposingLength);

                secondElement.Location = PointF.Empty.Displace(firstElement.Size, orientation);
                secondElement.Size = secondElement.Size.Scale(minOppostingLength / secondOpposingLength);
            }
            else if (elements.Count() > 2)
            {
                MutableRectangleF firstElement = elements.First();
                MutableRectangleF secondElement = elements.Skip(1).First();
                MutableRectangleF thirdElement = elements.Skip(2).First();

                var otherElements = elements.Skip(1);

                LayOut(otherElements, opposingOrientation);

                float firstOpposingLength = opposingOrientation.GetLength(firstElement.Size);
                float othersOpposingLength = opposingOrientation.GetLength(secondElement.Size)
                    + opposingOrientation.GetLength(thirdElement.Size);
                float minOppostingLength = Math.Min(firstOpposingLength, othersOpposingLength);

                firstElement.Location = PointF.Empty;
                firstElement.Size = firstElement.Size.Scale(minOppostingLength / firstOpposingLength);

                float othersScaleFactor = minOppostingLength / othersOpposingLength;

                foreach (MutableRectangleF element in otherElements)
                {
                    element.Location = element.Location
                        .Scale(othersScaleFactor)
                        .Displace(firstElement.Size, orientation);
                    element.Size = element.Size.Scale(othersScaleFactor);
                }
            }
            else throw new InvalidOperationException();
        }

        public static IReadOnlyCollection<RectangleF> Generate(IReadOnlyCollection<SizeF> sizes)
        {
            if (sizes.Count() < 2)
                return sizes.Select(s => new RectangleF(PointF.Empty, s)).ToArray();

            var elements = sizes
                .Select(s => new MutableRectangleF { Size = s })
                .ToArray();
            LayOut(elements, Orientation.Horizontal);
            return elements.Select(r => r.ToRectangleF()).ToArray();
        }
    }
}
