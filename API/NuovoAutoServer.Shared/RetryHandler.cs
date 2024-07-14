using Microsoft.Extensions.Logging;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace NuovoAutoServer.Shared
{
    public class RetryHandler
    {
        private readonly ILogger _logger;

        public RetryHandler(ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<RetryHandler>(); ;
        }

        public async Task<T> ExponentialRetry<T>(Func<Task<T>> operation, string callerFunctionName, int maxRetries = 3, int initialDelayMilliseconds = 1000)
        {
            int retryCount = 0;
            int delay = initialDelayMilliseconds;

            while (true)
            {
                try
                {
                    return await operation();
                }
                catch (Exception ex) when (retryCount < maxRetries)
                {
                    if (ex is HttpRequestException httpRequestException && ((httpRequestException.StatusCode >= HttpStatusCode.BadRequest && httpRequestException.StatusCode <= HttpStatusCode.Conflict) || httpRequestException.StatusCode >= HttpStatusCode.InternalServerError))
                    {
                        _logger.LogWarning($"Operation failed for {callerFunctionName}. Exiting from the retry handler.");
                        throw;
                    }

                    _logger.LogWarning(ex, $"Operation failed for {callerFunctionName}. Retrying in {delay}ms...");
                    await Task.Delay(delay);
                    retryCount++;
                    delay *= 2; // Exponential backoff
                }

            }
        }
    }
}
