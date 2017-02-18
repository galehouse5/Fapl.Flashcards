using ASF.Shared.Cram;
using ASF.Shared.Helpers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace ASF.Tests
{
    [TestClass]
    public class CramServiceTests
    {
        protected static string ApiClientID => ConfigurationManager.AppSettings["CramApiClientID"];
        protected static string ApiUsername => ConfigurationManager.AppSettings["CramApiUsername"];
        protected static string ApiPassword => ConfigurationManager.AppSettings["CramApiPassword"];
        protected static int TestSetID => int.Parse(ConfigurationManager.AppSettings["CramTestSetID"]);
        protected static string TestImageID => ConfigurationManager.AppSettings["CramTestImageID"];
        protected static string TestImageSrc => ConfigurationManager.AppSettings["CramTestImageSrc"];

        protected static byte[] GetTestImageData()
        {
            using (Stream data = Assembly.GetExecutingAssembly()
                .GetManifestResourceStream("ASF.Tests.logo-cram.png"))
            {
                return data.ReadAll();
            }
        }

        private Random random = new Random();
        private CramService service;

        [TestInitialize]
        public void Initialize()
        {
            if (service == null)
            {
                service = new CramService(ApiClientID);
                service.LogIn(ApiUsername, ApiPassword).Wait();
            }
        }

        [TestMethod]
        public async Task LogsIn()
        {
            service = new CramService(ApiClientID);
            await service.LogIn(ApiUsername, ApiPassword);

            Assert.IsNotNull(service.LoggedInUserID);
            Assert.AreEqual(ApiUsername, service.LoggedInUserName);
        }

        [TestMethod]
        public async Task GetsSet()
        {
            CramSet set = await service.GetSet(TestSetID);

            Assert.IsNotNull(set);
            Assert.IsNotNull(set.Title);
            Assert.IsNotNull(set.Description);
            Assert.IsNotNull(set.Subject);
            Assert.IsNotNull(set.Access);
            Assert.IsNotNull(set.LangFront);
            Assert.IsNotNull(set.LangBack);
            Assert.IsNotNull(set.LangHint);

            Assert.AreNotEqual(0, set.Cards.Count);
            foreach (CramCard card in set.Cards)
            {
                Assert.IsNotNull(card.Front);
                Assert.IsNotNull(card.Back);
            }
        }

        [TestMethod]
        public async Task UpdatesSet()
        {
            CramSet expectedSet = await service.GetSet(TestSetID);
            Assert.IsNotNull(expectedSet);

            expectedSet.Title = Guid.NewGuid().ToString();
            expectedSet.Subject = Guid.NewGuid().ToString();
            expectedSet.Description = Guid.NewGuid().ToString();
            await service.UpdateSet(expectedSet);

            CramSet actualSet = await service.GetSet(TestSetID);
            Assert.IsNotNull(actualSet);
            Assert.AreEqual(expectedSet.Title, actualSet.Title);
            Assert.AreEqual(expectedSet.Subject, actualSet.Subject);
            Assert.AreEqual(expectedSet.Description, actualSet.Description);
        }

        [TestMethod]
        public async Task UpdatesCard()
        {
            CramSet expectedSet = await service.GetSet(TestSetID);
            Assert.IsNotNull(expectedSet);
            Assert.AreNotEqual(0, expectedSet.Cards.Count);

            foreach (CramCard card in expectedSet.Cards)
            {
                card.Front = Guid.NewGuid().ToString();
                card.ImageFront = random.Next(0, 2) == 0 ? null : TestImageID;
                card.Back = Guid.NewGuid().ToString();
                card.ImageUrl = random.Next(0, 2) == 0 ? null : TestImageID;
                await service.UpdateCard(card, expectedSet);
            }

            CramSet actualSet = await service.GetSet(TestSetID);
            Assert.IsNotNull(actualSet);

            Assert.AreNotEqual(0, expectedSet.Cards.Count);
            foreach (CramCard actualCard in actualSet.Cards)
            {
                CramCard expectedCard = expectedSet.Cards
                    .SingleOrDefault(c => c.CardID == actualCard.CardID);
                Assert.IsNotNull(expectedCard);

                Assert.AreEqual(expectedCard.Front, actualCard.Front);
                Assert.AreEqual(expectedCard.ImageFront,
                    // For some reason this property is repurposed when read to supply an image's URL rather than its ID.
                    actualCard.ImageFront?.Replace(TestImageSrc, TestImageID));
                Assert.AreEqual(expectedCard.Back, actualCard.Back);
                Assert.AreEqual(expectedCard.ImageUrl,
                    // For some reason this property is repurposed when read to supply an image's URL rather than its ID.
                    actualCard.ImageUrl?.Replace(TestImageSrc, TestImageID));
            }
        }

        [TestMethod]
        public async Task AddsAndRemovesCard()
        {
            CramSet set = await service.GetSet(TestSetID);
            Assert.IsNotNull(set);

            CramCard card = new CramCard
            {
                Front = Guid.NewGuid().ToString(),
                ImageFront = random.Next(0, 2) == 0 ? null : TestImageID,
                Back = Guid.NewGuid().ToString(),
                ImageUrl = random.Next(0, 2) == 0 ? null : TestImageID
            };
            await service.AddCard(card, set);
            Assert.IsNotNull(card.CardID);
            CollectionAssert.Contains(set.Cards, card);

            await service.RemoveCard(card, set);
            Assert.IsNull(card.CardID);
            CollectionAssert.DoesNotContain(set.Cards, card);
        }

        [TestMethod]
        public async Task UploadsImage()
        {
            var expectedImageData = GetTestImageData();

            var images = await service.UploadImages(
                new KeyValuePair<string, byte[]>("logo-cram.png", expectedImageData));

            CramImage image = images.SingleOrDefault();
            Assert.IsNotNull(image);
            Assert.IsNotNull(image.ID);
            Assert.IsNotNull(image.Url);
        }
    }
}
