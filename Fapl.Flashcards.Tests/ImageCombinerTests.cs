using Fapl.Flashcards.Shared;
using Fapl.Flashcards.Shared.ImageLayout;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Fapl.Flashcards.Tests
{
    [TestClass]
    public class ImageCombinerTests
    {
        private static Image redImage, greenImage, blueImage, grayImage;

        public static void FillImage(Image image, Color color)
        {
            using (Graphics graphics = Graphics.FromImage(image))
            {
                graphics.Clear(color);
            }
        }

        [ClassInitialize]
        public static void Initialize(TestContext context)
        {
            redImage = new Bitmap(6, 6);
            FillImage(redImage, Color.Red);
            greenImage = new Bitmap(6, 6);
            FillImage(greenImage, Color.FromArgb(255, 0, 255, 0));
            blueImage = new Bitmap(6, 6);
            FillImage(blueImage, Color.Blue);
            grayImage = new Bitmap(6, 6);
            FillImage(grayImage, Color.Gray);
        }

        [ClassCleanup]
        public static void Cleanup()
        {
            redImage.Dispose();
            greenImage.Dispose();
            blueImage.Dispose();
            grayImage.Dispose();
        }

        protected Image GetExpectedImage(string resourceName)
        {
            using (Stream stream = Assembly.GetExecutingAssembly()
                .GetManifestResourceStream(resourceName))
            {
                return Image.FromStream(stream);
            }
        }

        protected void AssertImagesAreEqual(Bitmap expected, Bitmap actual)
        {
            var expectedPixels =
                from x in Enumerable.Range(0, expected.Width)
                from y in Enumerable.Range(0, expected.Height)
                let color = expected.GetPixel(x, y)
                select new { color.R, color.G, color.B };
            var actualPixels =
                from x in Enumerable.Range(0, actual.Width)
                from y in Enumerable.Range(0, actual.Height)
                let color = actual.GetPixel(x, y)
                select new { color.R, color.G, color.B };
            CollectionAssert.AreEqual(expectedPixels.ToArray(), actualPixels.ToArray());
        }

        [TestMethod]
        public void CombinesOneImage()
        {
            using (Image expectedImage = GetExpectedImage("Fapl.Flashcards.Tests.ImageCombiner.one-image-combo.png"))
            using (Image actualImage = new[] { redImage }
                .CombineImages(RecursiveShrinkingImageLayout.Generate))
            {
                AssertImagesAreEqual((Bitmap)expectedImage, (Bitmap)actualImage);
            }
        }

        [TestMethod]
        public void CombinesTwoImages()
        {
            using (Image expectedImage = GetExpectedImage("Fapl.Flashcards.Tests.ImageCombiner.two-image-combo.png"))
            using (Image actualImage = new[] { redImage, greenImage }
                .CombineImages(RecursiveShrinkingImageLayout.Generate))
            {
                AssertImagesAreEqual((Bitmap)expectedImage, (Bitmap)actualImage);
            }
        }

        [TestMethod]
        public void CombinesThreeImages()
        {
            using (Image expectedImage = GetExpectedImage("Fapl.Flashcards.Tests.ImageCombiner.three-image-combo.png"))
            using (Image actualImage = new[] { redImage, greenImage, blueImage }
                .CombineImages(RecursiveShrinkingImageLayout.Generate))
            {
                AssertImagesAreEqual((Bitmap)expectedImage, (Bitmap)actualImage);
            }
        }

        [TestMethod]
        public void CombinesFourImages()
        {
            using (Image expectedImage = GetExpectedImage("Fapl.Flashcards.Tests.ImageCombiner.four-image-combo.png"))
            using (Image actualImage = new[] { redImage, greenImage, blueImage, grayImage }
                .CombineImages(RecursiveShrinkingImageLayout.Generate))
            {
                AssertImagesAreEqual((Bitmap)expectedImage, (Bitmap)actualImage);
            }
        }
    }
}
