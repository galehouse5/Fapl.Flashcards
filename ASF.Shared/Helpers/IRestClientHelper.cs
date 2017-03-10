using RestSharp;
using System.Net;
using System.Threading.Tasks;

namespace ASF.Shared.Helpers
{
    public static class IRestClientHelper
    {
        public static async Task<IRestResponse> ExecuteTaskAsyncWithRetry(this IRestClient client, IRestRequest request)
        {
            try
            {
                return await client.ExecuteTaskAsync(request);
            }
            catch (WebException ex) when (ex.Status == WebExceptionStatus.ConnectionClosed)
            {
                return await client.ExecuteTaskAsync(request);
            }
        }

        public static async Task<IRestResponse<T>> ExecuteTaskAsyncWithRetry<T>(this IRestClient client, IRestRequest request)
        {
            try
            {
                return await client.ExecuteTaskAsync<T>(request);
            }
            catch (WebException ex) when (ex.Status == WebExceptionStatus.ConnectionClosed)
            {
                return await client.ExecuteTaskAsync<T>(request);
            }
        }
    }
}
