using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using NuovoAutoServer.Shared;
using NuovoAutoServer.Shared.CustomExceptions;

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NuovoAutoServer.Api.Extensions
{
    public class SecurityService
    {
        private readonly ILogger _logger;
        private readonly AppSettings _appSettings;
        private readonly string[] _whitelistedIPs;

        private static readonly ConcurrentDictionary<string, (DateTimeOffset, int)> _rateLimits = new();

        public SecurityService(ILoggerFactory loggerFactory, IOptions<AppSettings> appSettings)
        {
            _logger = loggerFactory.CreateLogger<SecurityService>();
            _appSettings = appSettings.Value;


            // Get the whitelisted IPs from an environment variable
            var whitelistedIPs = _appSettings.RateLimiting.WhitelistedIPs;
            _whitelistedIPs = whitelistedIPs.Split(';');
        }

        public bool ValidateClientIp(HttpRequestData req)
        {
            // Extract the caller's IP address from the request.
            var callerIP = GetClientIpAddress(req);
            if (callerIP == null) return false;

            var isIPWhiteListed = IsCallerIPWhitelisted(callerIP);
            if (isIPWhiteListed) return true;

            var isLimitExceeded = IsRateLimitExceeded(callerIP);
            if (isLimitExceeded)
            {
                _logger.LogWarning("Rate limit exceeded for IP: {callerIP}", callerIP);
                throw new RateLimitExceededException($"Rate limit exceeded.");
            }
            return true;
        }

        private bool IsRateLimitExceeded(string callerIP)
        {
            var now = DateTimeOffset.UtcNow;
            var period = TimeSpan.FromHours(this._appSettings.RateLimiting.WindowDurationInHours);
            var allowedRequests = this._appSettings.RateLimiting.RequestsLimit;

            var keyExists = _rateLimits.TryGetValue(callerIP, out var entry);
            if (!keyExists || now - entry.Item1 > period)
            {
                // Reset count for new period
                _rateLimits[callerIP] = (now, 1);
                return false;
            }
            else if (entry.Item2 < allowedRequests)
            {
                // Increment request count
                _rateLimits[callerIP] = (entry.Item1, entry.Item2 + 1);
                return false;
            }
            else
            {
                return true;
            }
        }

        public bool IsCallerIPWhitelisted(string callerIP)
        {
            // Check if the caller's IP is in the list of whitelisted IPs
            return callerIP != null && _whitelistedIPs.Contains(callerIP);
        }


        private string? GetClientIpAddress(HttpRequestData req)
        {
            string? ip = "";
            if (req.Headers.TryGetValues("X-Forwarded-For", out var forwardedFor))
            {
                ip = forwardedFor.FirstOrDefault()?.Split(',').FirstOrDefault()?.Trim()?.Split(':').FirstOrDefault()?.Trim();
                _logger.LogInformation("Client IP Address: {ip}", ip);
            }
           if(string.IsNullOrEmpty(ip)) throw new IPNotFoundException("No IP address found in the request headers.");
            return ip;
        }
    }
}
