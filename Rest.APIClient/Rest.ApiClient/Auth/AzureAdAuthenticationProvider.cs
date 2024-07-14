using Microsoft.Identity.Client;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Authentication;
using System.Text;
using System.Threading.Tasks;

namespace Rest.ApiClient.Auth
{
    /// <summary>
    /// AzureAdAuthenticationProvider authenticates using azure service principal.Make sure the settings AUTHORITY_URI, AZURE_TENANT_ID, AZURE_CLIENT_ID, AZURE_CLIENT_SECRET, SERVICE_PRINCIPAL_SCOPES are configured in the app settings.
    /// </summary>
    public class AzureAdAuthenticationProvider : IAuthenticationProvider
    {
        private readonly IConfidentialClientApplication _app;
        private readonly string[] _scopes;

        public AzureAdAuthenticationProvider()
        {
            var clientId = Environment.GetEnvironmentVariable("AZURE_CLIENT_ID");
            var clientSecret = Environment.GetEnvironmentVariable("AZURE_CLIENT_SECRET");
            var authority = string.Format(Environment.GetEnvironmentVariable("AUTHORITY_URI")?.ToString() ?? "",
                Environment.GetEnvironmentVariable("AZURE_TENANT_ID")?.ToString() ?? "");

            var scopesString = Environment.GetEnvironmentVariable("SERVICE_PRINCIPAL_SCOPES");
            string[] scopes = scopesString?.Split(';') ?? new string[] { };

            if (!string.IsNullOrEmpty(clientId) && !string.IsNullOrEmpty(clientSecret))
                _app = ConfidentialClientApplicationBuilder.Create(clientId)
                    .WithClientSecret(clientSecret)
                    .WithAuthority(new Uri(authority))
                    .Build();
            _scopes = scopes;
        }

        public async Task<HttpRequestMessage> AcquireAndSetAuthenticationHeaderAsync(HttpRequestMessage httpRequestMessage)
        {
            var token = await this.AcquireTokenAsync();
            httpRequestMessage.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            return httpRequestMessage;
        }

        public async Task<string> AcquireTokenAsync()
        {
            try
            {
                var result = await _app.AcquireTokenForClient(_scopes).ExecuteAsync();
                return result.AccessToken;
            }
            catch (MsalServiceException ex)
            {
                // Handle exceptions related to Azure AD service (e.g., unavailable)
                throw new AuthenticationException("Azure AD service exception occurred.", ex);
            }
            catch (MsalClientException ex)
            {
                // Handle client exceptions (e.g., configuration issues)
                throw new AuthenticationException("Azure AD client exception occurred.", ex);
            }
        }
    }
}
