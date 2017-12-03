using System.Collections.Generic;
using System.Threading.Tasks;

namespace Fapl.Flashcards.Shared
{
    public interface IPetService
    {
        Task<IReadOnlyCollection<IPet>> GetPets(string shelterID, PetStatus status);
    }

    public static class IPetServiceExtensions
    {
        public static async Task<IReadOnlyCollection<IPet>> GetPets(this IPetService service,
            string shelterID, IReadOnlyCollection<PetStatus> statuses)
        {
            var pets = new List<IPet>();

            foreach (PetStatus status in statuses)
            {
                pets.AddRange(await service.GetPets(shelterID, status));
            }

            return pets.AsReadOnly();
        }
    }
}
