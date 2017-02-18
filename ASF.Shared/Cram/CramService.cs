using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace ASF.Shared.Cram
{
    public class CramService : IFlashcardService
    {
        private RestClient client = new RestClient("https://api.Cram.com/v2");
        private string clientID;
        private dynamic loginDto;

        public CramService(string clientID)
        {
            this.clientID = clientID;
        }

        protected void AssertOkResponse(IRestResponse response)
        {
            if (response.ErrorException != null)
                throw response.ErrorException;

            var responseWithData = response as IRestResponse<dynamic>;
            dynamic data = responseWithData?.Data;

            if (response.StatusCode == HttpStatusCode.BadRequest)
                throw new Exception(data?["error_description"]);

            int? status = (data?.ContainsKey("status") ?? false) ?
                (int)data["status"] : (int?)null;
            if (response.StatusCode != HttpStatusCode.OK
                && response.StatusCode != HttpStatusCode.Created
                || status.HasValue && status != 1)
                throw new Exception("Unexpected response.");
        }

        public int? LoggedInUserID => (int?)loginDto?["user_id"];
        public string LoggedInUserName => loginDto?["user_name"];

        public async Task LogIn(string username, string password)
        {
            if (loginDto != null)
                throw new InvalidOperationException();

            RestRequest request = new RestRequest("user/login", Method.POST);
            request.AddParameter("username", username);
            request.AddParameter("password", password);
            request.AddParameter("client_id", clientID);

            var response = await client.ExecuteTaskAsync<dynamic>(request);
            AssertOkResponse(response);

            client.AddDefaultHeader("Authorization", $"Bearer {response.Data["access_token"]}");
            loginDto = response.Data;
        }

        public void LogOut()
        {
            if (loginDto == null)
                throw new InvalidOperationException();

            client.RemoveDefaultParameter("Authorization");
            loginDto = null;
        }

        public async Task<CramSet> GetSet(int id)
        {
            if (loginDto == null)
                throw new InvalidOperationException();

            RestRequest request = new RestRequest("sets/{id}");
            request.AddUrlSegment("id", id.ToString());

            var response = await client.ExecuteTaskAsync<List<CramSet>>(request);
            AssertOkResponse(response);

            return response.Data.SingleOrDefault();
        }

        public async Task UpdateSet(CramSet set)
        {
            if (loginDto == null)
                throw new InvalidOperationException();

            RestRequest request = new RestRequest("sets/{id}", Method.POST);
            request.AddUrlSegment("id", set.SetID.ToString());
            request.AddParameter("title", set.Title);
            request.AddParameter("description", set.Description);
            request.AddParameter("subject[]", set.Subject);
            request.AddParameter("access", set.Access);
            request.AddParameter("lang_front", set.LangFront);
            request.AddParameter("lang_back", set.LangBack);
            request.AddParameter("lang_hint", set.LangHint);

            var response = await client.ExecuteTaskAsync(request);
            AssertOkResponse(response);
        }

        protected void AddParameters(CramCard card, RestRequest request)
        {
            Uri _;
            Func<string, string> nullIfUrl = s =>
                Uri.TryCreate(s ?? string.Empty, UriKind.Absolute, out _) ? null : s;

            request.AddParameter("html", 1);
            request.AddParameter("front", card.Front ?? string.Empty);
            request.AddParameter("back", card.Back ?? string.Empty);
            request.AddParameter("hint", card.Hint ?? string.Empty);
            request.AddParameter("image_id", nullIfUrl(card.ImageUrl ?? "null"));
            request.AddParameter("front_image_id", nullIfUrl(card.ImageFront ?? "null"));
            request.AddParameter("hint_image_id", nullIfUrl(card.ImageHint ?? "null"));
        }

        public async Task UpdateCard(CramCard card, CramSet set)
        {
            if (loginDto == null)
                throw new InvalidOperationException();

            RestRequest request = new RestRequest("sets/{setID}/cards/{cardID}", Method.PUT);
            request.AddUrlSegment("setID", set.SetID.ToString());
            request.AddUrlSegment("cardID", card.CardID.ToString());
            AddParameters(card, request);

            var response = await client.ExecuteTaskAsync<dynamic>(request);
            AssertOkResponse(response);
        }

        public async Task AddCard(CramCard card, CramSet set)
        {
            if (card.CardID.HasValue || set.Cards.Contains(card))
                throw new ArgumentException();

            if (loginDto == null)
                throw new InvalidOperationException();

            RestRequest request = new RestRequest("sets/{setID}/cards", Method.POST);
            request.AddUrlSegment("setID", set.SetID.ToString());
            AddParameters(card, request);

            var response = await client.ExecuteTaskAsync<dynamic>(request);
            AssertOkResponse(response);

            card.CardID = int.Parse(response.Data["card_id"]);
            set.Cards.Add(card);
        }

        public async Task RemoveCard(CramCard card, CramSet set)
        {
            if (!card.CardID.HasValue || !set.Cards.Contains(card))
                throw new ArgumentException();

            if (loginDto == null)
                throw new InvalidOperationException();

            // Don't remove the final card.  There's a bug in the Cram API that prevents you from adding cards
            // when no cards exist.  A set in this state can be repaired by adding cards through their website.
            if (set.Cards.First().Equals(card) && set.Cards.Last().Equals(card))
                throw new NotSupportedException("Unable to remove the final card in a set, since it will corrupt the set.");

            RestRequest request = new RestRequest("sets/{setID}/cards/{cardID}", Method.DELETE);
            request.AddUrlSegment("setID", set.SetID.ToString());
            request.AddUrlSegment("cardID", card.CardID.ToString());

            var response = await client.ExecuteTaskAsync<dynamic>(request);
            AssertOkResponse(response);

            card.CardID = null;
            set.Cards.Remove(card);
        }

        public async Task<IReadOnlyCollection<CramImage>> UploadImages(params KeyValuePair<string, byte[]>[] images)
        {
            if (!images.Any())
                throw new ArgumentException();

            RestRequest request = new RestRequest("images", Method.POST);
            foreach (var image in images)
            {
                string contentType = image.Key.EndsWith(".jpg") ? "image/jpeg"
                    : image.Key.EndsWith(".jpeg") ? "image/jpeg"
                    : image.Key.EndsWith(".gif") ? "image/gif"
                    : image.Key.EndsWith(".png") ? "image/png"
                    : null;
                if (contentType == null)
                    throw new ArgumentException($"Unsupported file extension: '{image.Key}'.", nameof(images));

                request.AddFile("image_data[]", image.Value, image.Key, contentType);
            }

            var response = await client.ExecuteTaskAsync<dynamic>(request);
            AssertOkResponse(response);

            return ((List<dynamic>)response.Data["images"])
                .Select(i => new CramImage { ID = i["id"], Url = i["url"] })
                .ToArray();
        }

        async Task<IFlashcardSet> IFlashcardService.GetSet(string id) => await GetSet(int.Parse(id));
        Task IFlashcardService.Save(IFlashcardSet set) => UpdateSet((CramSet)set);
        Task IFlashcardService.Add(IFlashcard card, IFlashcardSet set) => AddCard((CramCard)card, (CramSet)set);
        Task IFlashcardService.Remove(IFlashcard card, IFlashcardSet set) => RemoveCard((CramCard)card, (CramSet)set);
        Task IFlashcardService.Save(IFlashcard card, IFlashcardSet set) => UpdateCard((CramCard)card, (CramSet)set);

        async Task<string> IFlashcardService.UploadImage(string contentType, byte[] data)
        {
            string fileExtension = contentType.Equals("image/jpeg") ? "jpg"
                : contentType.Equals("image/gif") ? "gif"
                : contentType.Equals("image/png") ? "png"
                : null;
            if (fileExtension == null)
                throw new ArgumentException($"Unsupported content type: '{contentType}'.", nameof(contentType));

            var images = await UploadImages(new KeyValuePair<string, byte[]>($"{Guid.NewGuid()}.{fileExtension}", data));
            return images.SingleOrDefault()?.ID;
        }
    }
}
