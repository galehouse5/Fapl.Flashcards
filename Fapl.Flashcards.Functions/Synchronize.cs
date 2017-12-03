using Fapl.Flashcards.Shared;
using Fapl.Flashcards.Shared.Cram;
using Fapl.Flashcards.Shared.ImageLayout;
using Fapl.Flashcards.Shared.Petfinder;
using Fapl.Flashcards.Shared.SyncData;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace Fapl.Flashcards.AzureFunctions
{
    public static class Synchronize
    {
        private static string petfinderApiKey => ConfigurationManager.AppSettings["PetfinderApiKey"];
        private static string cramApiClientID => ConfigurationManager.AppSettings["CramApiClientID"];
        private static string cramApiUsername => ConfigurationManager.AppSettings["CramApiUsername"];
        private static string cramApiPassword => ConfigurationManager.AppSettings["CramApiPassword"];
        private static string azureWebJobsStorage => ConfigurationManager.AppSettings["AzureWebJobsStorage"];
        private static string flashcardSetID => ConfigurationManager.AppSettings["FlashcardSetID"];
        private static string flashcardSetTitle => ConfigurationManager.AppSettings["FlashcardSetTitle"];
        private static string flashcardSetDescription => ConfigurationManager.AppSettings["FlashcardSetDescription"];
        private static string shelterID => ConfigurationManager.AppSettings["ShelterID"];

        private static TimeZoneInfo shelterTimeZone =>
            TimeZoneInfo.FindSystemTimeZoneById(ConfigurationManager.AppSettings["ShelterTimeZone"]);

        private static IEnumerable<PetType> flashcardPetTypes => ConfigurationManager.AppSettings["FlashcardPetTypes"]
            .Split(new[] { ", ", "," }, StringSplitOptions.RemoveEmptyEntries)
            .Select(v => (PetType)Enum.Parse(typeof(PetType), v));

        private static IEnumerable<PetAge> flashcardPetAges => ConfigurationManager.AppSettings["FlashcardPetAges"]
            .Split(new[] { ", ", "," }, StringSplitOptions.RemoveEmptyEntries)
            .Select(v => (PetAge)Enum.Parse(typeof(PetAge), v));

        [FunctionName("Synchronize")]
        public static async Task Run(
            [TimerTrigger("0 0 */1 * * *")]TimerInfo myTimer,
            TraceWriter log)
        {
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

            var petService = new PetfinderService(petfinderApiKey);
            var flashcardService = new CramService(cramApiClientID);

            log.Info("Logging in to flashcard service...");
            await flashcardService.LogIn(cramApiUsername, cramApiPassword);
            var syncService = new AzureBlobSyncService(azureWebJobsStorage);
            GenerateImageLayout imageLayoutGenerator = RecursiveShrinkingImageLayout.Generate;

            var synchronizer = new Synchronizer(
                petService, flashcardService, syncService, imageLayoutGenerator)
            {
                Logger = m => log.Info(m)
            };
            await synchronizer.Synchronize(flashcardSetID, shelterID, new SyncOptions
            {
                PetFilter = p =>
                    (!flashcardPetTypes.Any() || flashcardPetTypes.Contains(p.Type))
                    && (!flashcardPetAges.Any() || flashcardPetAges.Contains(p.Age)),
                FlashcardSetTitle = string.Format(flashcardSetTitle,
                    TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, shelterTimeZone)),
                FlashcardSetDescription = flashcardSetDescription
            });
        }
    }
}
