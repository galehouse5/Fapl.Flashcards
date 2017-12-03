using Fapl.Flashcards.Shared.Petfinder;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Configuration;
using System.Threading.Tasks;

namespace Fapl.Flashcards.Tests
{
    [TestClass]
    public class PetfinderServiceTests
    {
        protected static string ApiKey => ConfigurationManager.AppSettings["PetfinderApiKey"];

        private PetfinderService service;

        [TestInitialize]
        public void Initialize()
        {
            service = new PetfinderService(ApiKey);
        }

        [TestMethod]
        public async Task GetsPets()
        {
            var pets = await service.GetPets("OH166", count: 1000);
            Assert.AreNotEqual(0, pets.Length);
        }
    }
}
