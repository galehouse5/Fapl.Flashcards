using FriendshipApl.Flashcards.Shared.Helpers;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;

namespace FriendshipApl.Flashcards.Shared
{
    public class Synchronizer
    {
        private IPetService petService;
        private IFlashcardService flashcardService;
        private ISyncService syncService;
        private GenerateImageLayout imageLayoutGenerator;

        public Synchronizer(
            IPetService petService,
            IFlashcardService flashcardService,
            ISyncService syncService,
            GenerateImageLayout imageLayoutGenerator)
        {
            this.petService = petService;
            this.flashcardService = flashcardService;
            this.syncService = syncService;
            this.imageLayoutGenerator = imageLayoutGenerator;
        }

        public Action<string> Logger { get; set; }

        public async Task InitializeFlashcard(IFlashcard flashcard, IPet pet, SyncOptions options)
        {
            flashcard.SetDescription(pet);

            using (Image image = await pet.GenerateCompositeImage(
                imageLayoutGenerator, options.FlashcardImageMargin, options.FlashcardImageBackgroundColor))
            {
                var imageData = image.SaveToBytes(options.FlashcardImageFormat);
                string imageID = await flashcardService.UploadImage(
                    options.FlashcardImageFormat.GetMimeType(), imageData);

                flashcard.SetImage(imageID);
            }
        }

        public virtual async Task AddFlashcard(
            IFlashcardSet set,
            IPet pet,
            SyncMetadata metadata,
            SyncOptions options)
        {
            Logger?.Invoke($"Adding flashcard for {pet.Name} ({pet.ID})...");

            IFlashcard flashcard = set.CreateFlashcard();
            await InitializeFlashcard(flashcard, pet, options);
            await flashcardService.Add(flashcard, set);

            metadata.LinkAndSetLastUpdateTimestamp(pet, flashcard);
        }

        public virtual async Task UpdateFlashcard(
            IFlashcardSet set,
            IFlashcard flashcard,
            IPet pet,
            SyncMetadata metadata,
            SyncOptions options)
        {
            Logger?.Invoke($"Updating flashcard for {pet.Name} ({pet.ID})...");

            await InitializeFlashcard(flashcard, pet, options);
            await flashcardService.Save(flashcard, set);

            metadata.LinkAndSetLastUpdateTimestamp(pet, flashcard);
        }

        public virtual async Task RemoveFlashcard(IFlashcardSet set, IFlashcard flashcard, SyncMetadata metadata)
        {
            Logger?.Invoke($"Removing flashcard {flashcard.ID}...");

            metadata.Remove(flashcard);
            // Order is important.  The method above uses the flashcard's ID and the method below clears it.
            await flashcardService.Remove(flashcard, set);
        }

        public async Task SynchronizeFlashcards(
            IFlashcardSet set,
            IReadOnlyCollection<IPet> pets,
            SyncMetadata metadata,
            SyncOptions options)
        {
            Logger?.Invoke($"Synchronizing flashcards for {pets.Count} pet(s)...");

            var indexedPets = pets.ToDictionary(p => p.ID);
            var indexedFlashcards = set.ToDictionary(f => f.ID);

            foreach (IPet pet in pets
                .Where(p => metadata.GetFlashcard(p, indexedFlashcards) == null))
            {
                await AddFlashcard(set, pet, metadata, options);
                await syncService.Save(metadata);
            }

            foreach (var pair in
                from pet in pets
                let flashcard = metadata.GetFlashcard(pet, indexedFlashcards)
                where flashcard != null
                select new { pet, flashcard })
            {
                if (metadata.GetLastUpdateTimestamp(pair.pet) >= pair.pet.LastUpdateTimestamp)
                    continue;

                await UpdateFlashcard(set, pair.flashcard, pair.pet, metadata, options);
                await syncService.Save(metadata);
            }

            foreach (IFlashcard flashcard in indexedFlashcards.Values
                .Where(f => metadata.GetPet(f, indexedPets) == null))
            {
                await RemoveFlashcard(set, flashcard, metadata);
                await syncService.Save(metadata);
            }
        }

        public async Task Synchronize(string flashcardSetID, string shelterID, SyncOptions options)
        {
            Logger?.Invoke("Retrieving flashcards...");
            var set = (await flashcardService.GetSet(flashcardSetID));

            Logger?.Invoke("Retrieving pets...");
            var pets = (await petService.GetPets(shelterID, options.PetStatuses))
                .Where(options.PetFilter)
                .ToArray();

            Logger?.Invoke("Retrieving metadata...");
            SyncMetadata metadata = await syncService.GetMetadata(flashcardSetID, shelterID)
                ?? new SyncMetadata(flashcardSetID, shelterID);

            await SynchronizeFlashcards(set, pets, metadata, options);

            Logger?.Invoke("Updating flashcard set...");
            set.Title = options.FlashcardSetTitle ?? set.Title;
            set.Tags = options.FlashcardSetTags ?? set.Tags;
            set.Description = options.FlashcardSetDescription ?? set.Description;
            await flashcardService.Save(set);
        }
    }
}
