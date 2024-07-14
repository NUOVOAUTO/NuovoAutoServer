using Microsoft.Extensions.DependencyInjection;

using Polly;
using Polly.CircuitBreaker;
using Polly.Extensions.Http;

using System;
using System.Collections.Generic;
using System.Diagnostics.Metrics;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Rest.ApiClient.Extensions
{
    public static class HttpClientBuilderExtensions
    {
        public static IHttpClientBuilder AddRetryPolicy(this IHttpClientBuilder builder)
        {
            return builder.AddPolicyHandler(GetRetryPolicy())
                .AddPolicyHandler(GetCircuitBreakerPolicy()); ;
        }

        /// <summary>
        /// The retry policy is configured with exponential backoff. It retries the request up to a maximum of 5 times, with increasing delays between retries. The goal of the retry policy is to give the system a chance to recover from transient issues without overwhelming the service.
        /// </summary>
        /// <example>
        ///The delay between retries follows the formula: TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)). For example:
        ///First retry: 2 seconds
        ///Second retry: 4 seconds
        ///Third retry: 8 seconds
        ///And so on, doubling the delay each time.
        /// </example>
        /// <returns>HttpResponseMessage</returns>
        static IAsyncPolicy<HttpResponseMessage> GetRetryPolicy()
        {
            return HttpPolicyExtensions
                .HandleTransientHttpError()
                .WaitAndRetryAsync(10, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)));
        }

        /// <summary>
        /// The CircuitBreaker is configured for 30 secs. And will kick in after failing 5 events.
        /// </summary>
        /// <returns>HttpResponseMessage</returns>
        static IAsyncPolicy<HttpResponseMessage> GetCircuitBreakerPolicy()
        {
            return HttpPolicyExtensions
                .HandleTransientHttpError()
                .CircuitBreakerAsync(5, TimeSpan.FromSeconds(30));
        }
    }
}
