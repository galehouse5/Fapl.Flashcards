using FriendshipApl.Flashcards.Shared.Helpers;
using RestSharp;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FriendshipApl.Flashcards.Shared.Petfinder
{
    public class PetfinderService : IPetService
    {
        private RestClient client;

        public PetfinderService(string key)
        {
            client = new RestClient("https://api.petfinder.com");
            client.AddDefaultParameter("key", key);
            client.AddHandler("text/xml", new CustomXmlDeserializer());
        }

        public async Task<petfinderPetRecord[]> GetPets(string shelterID,
            int offset = 0, int count = 25, petStatusType status = petStatusType.A)
        {
            RestRequest request = new RestRequest("shelter.getPets");
            request.AddParameter("id", shelterID);
            request.AddParameter("offset", offset);
            request.AddParameter("count", count);
            request.AddParameter("status", status);

            var response = await client.ExecuteTaskAsyncWithRetry<petfinder>(request);
            if (response.ErrorException != null)
                throw response.ErrorException;

            return (response.Data.Item as petfinderPetRecordList)?.pet;
        }

        async Task<IReadOnlyCollection<IPet>> IPetService.GetPets(string shelterID, PetStatus status)
        {
            petStatusType status2 =
                status == PetStatus.Adoptable ? petStatusType.A
                : status == PetStatus.AdoptedOrRemoved ? petStatusType.X
                : status == PetStatus.Hold ? petStatusType.H
                : petStatusType.P;

            return (await GetPets(shelterID, 0, 1000, status2))
                .Select(p => (IPet)new PetfinderPetAdapter(p))
                .ToArray();
        }
    }
}
