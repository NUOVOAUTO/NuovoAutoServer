using System;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace NuovoAutoServer.Api
{
    public class AfShared
    {
        private readonly ILogger _logger;

        public AfShared(ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<AfShared>();
        }

        [Function("TfKeepFunctionAppActive")]
        public void TfKeepFunctionAppActive([TimerTrigger("0 */30 * * * *")] TimerInfo myTimer)
        {
            _logger.LogInformation($"C# Timer trigger function executed at: {DateTime.Now}");

            _logger.LogInformation($"Running this function to keep it active");
            if (myTimer.ScheduleStatus is not null)
            {
                _logger.LogInformation($"Next timer schedule at: {myTimer.ScheduleStatus.Next}");
            }
        }
    }
}
