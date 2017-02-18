using ASF.Shared.Helpers;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;

namespace ASF.Shared
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
            await InitializeFlashcard(flashcard, pet, options);
            await flashcardService.Save(flashcard, set);

            metadata.LinkAndSetLastUpdateTimestamp(pet, flashcard);
        }

        public virtual async Task RemoveFlashcard(IFlashcardSet set, IFlashcard flashcard, SyncMetadata metadata)
        {
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
            var set = (await flashcardService.GetSet(flashcardSetID));
            var pets = (await petService.GetPets(shelterID, options.PetStatuses))
                .Where(options.PetFilter)
                .ToArray();
            SyncMetadata metadata = await syncService.GetMetadata(flashcardSetID, shelterID)
                ?? new SyncMetadata(flashcardSetID, shelterID);
            await SynchronizeFlashcards(set, pets, metadata, options);

            set.Title = options.FlashcardSetTitle ?? set.Title;
            set.Tags = options.FlashcardSetTags ?? set.Tags;
            set.Description = options.FlashcardSetDescription ?? set.Description;
            await flashcardService.Save(set);
        }
    }
}
