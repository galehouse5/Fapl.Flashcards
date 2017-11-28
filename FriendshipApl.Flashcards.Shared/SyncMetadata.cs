using System;
using System.Collections.Generic;

namespace FriendshipApl.Flashcards.Shared
{
    public class SyncMetadata
    {
        public SyncMetadata()
        { }

        public SyncMetadata(string flashcardSetID, string shelterID)
        {
            this.FlashcardSetID = flashcardSetID;
            this.ShelterID = shelterID;
        }

        public string FlashcardSetID { get; set; }
        public string ShelterID { get; set; }
        public IDictionary<string, string> FlashcardIDs { get; set; } = new Dictionary<string, string>();
        public IDictionary<string, DateTime> PetLastUpdateTimestamps { get; set; } = new Dictionary<string, DateTime>();
        public IDictionary<string, string> PetIDs { get; set; } = new Dictionary<string, string>();

        protected string GetFlashcardID(IPet pet)
            => FlashcardIDs.ContainsKey(pet.ID) ? FlashcardIDs[pet.ID] : null;

        protected string GetPetID(IFlashcard flashcard)
            => PetIDs.ContainsKey(flashcard.ID) ? PetIDs[flashcard.ID] : null;

        public DateTime? GetLastUpdateTimestamp(IPet pet)
            => PetLastUpdateTimestamps.ContainsKey(pet.ID) ? PetLastUpdateTimestamps[pet.ID] : (DateTime?)null;

        public IFlashcard GetFlashcard(IPet pet, IDictionary<string, IFlashcard> flashcards)
        {
            string flashcardID = GetFlashcardID(pet);
            if (flashcardID == null) return null;

            return flashcards.ContainsKey(flashcardID) ? flashcards[flashcardID] : null;
        }

        public IPet GetPet(IFlashcard flashcard, IDictionary<string, IPet> pets)
        {
            string petID = GetPetID(flashcard);
            if (petID == null) return null;

            return pets.ContainsKey(petID) ? pets[petID] : null;
        }

        public void LinkAndSetLastUpdateTimestamp(IPet pet, IFlashcard flashcard)
        {
            if (string.IsNullOrEmpty(pet.ID))
                throw new ArgumentException("Pet must have an ID.", nameof(pet));

            if (string.IsNullOrEmpty(flashcard.ID))
                throw new ArgumentException("Flashcard must have an ID.", nameof(flashcard));

            FlashcardIDs[pet.ID] = flashcard.ID;
            PetLastUpdateTimestamps[pet.ID] = pet.LastUpdateTimestamp;
            PetIDs[flashcard.ID] = pet.ID;
        }

        public void Remove(IFlashcard flashcard)
        {
            if (string.IsNullOrEmpty(flashcard.ID))
                throw new ArgumentException("Flashcard must have an ID.", nameof(flashcard));

            string petID = GetPetID(flashcard);
            FlashcardIDs.Remove(petID ?? string.Empty);
            PetLastUpdateTimestamps.Remove(petID ?? string.Empty);
            PetIDs.Remove(flashcard.ID);
        }
    }
}
