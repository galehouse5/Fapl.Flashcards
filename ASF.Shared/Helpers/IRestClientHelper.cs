using Polly;
using RestSharp;
using System;
using System.Threading.Tasks;

namespace ASF.Shared.Helpers
{
    public static class IRestClientHelper
    {
        public static Policy<IRestResponse> RetryPolicy { get; set; } = Policy
            .HandleResult<IRestResponse>(r => r.ResponseStatus == ResponseStatus.Error)
            .WaitAndRetryAsync(5, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)));

        public static Task<IRestResponse> ExecuteTaskAsyncWithRetry(this IRestClient client, IRestRequest request)
            => RetryPolicy
            .ExecuteAsync(() => client.ExecuteTaskAsync(request));

        public static async Task<IRestResponse<T>> ExecuteTaskAsyncWithRetry<T>(this IRestClient client, IRestRequest request)
            => (IRestResponse<T>)await RetryPolicy
            .ExecuteAsync(async () => await client.ExecuteTaskAsync<T>(request));
    }
}
