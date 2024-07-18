using Rest.ApiClient.Auth;
using Rest.ApiClient.Model;

using System.Security.Authentication;
using System.Text.Json;

namespace Rest.ApiClient
{
    public class ApiClient : IApiClient
    {
        private readonly HttpClient _httpClient;
        private readonly IAuthenticationProvider _azureAdAuthenticationProvider;
        private readonly IAuthenticationProvider _customAuthenticationHeaderProvider;

        public ApiClient(HttpClient httpClient, AzureAdAuthenticationProvider? azureAdAuthenticationProvider, CustomAuthenticationHeaderProvider? customAuthenticationHeaderProvider)
        {
            _httpClient = httpClient;
            _azureAdAuthenticationProvider = azureAdAuthenticationProvider;
            _customAuthenticationHeaderProvider = customAuthenticationHeaderProvider;
        }

        public async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, AuthenticationKind authenticationKind)
        {
            var _authenticationProvider = GetAuthenticationProvider(authenticationKind);

            if (_authenticationProvider != null)
            {
                request = await _authenticationProvider.AcquireAndSetAuthenticationHeaderAsync(request);
            }
            var response = await _httpClient.SendAsync(request);
            try
            {
                response.EnsureSuccessStatusCode();
            }
            catch (Exception ex)
            {
                var errorMessage = await response.Content.ReadAsStringAsync();
                //var rem = new ApiClientErrorModel()
                //{
                //    StatusCode = response.StatusCode,
                //    Message = errorMessage != null && errorMessage.StartsWith('{') && errorMessage.EndsWith('}') ? JsonSerializer.Deserialize<object>(errorMessage) : errorMessage,
                //    Description = "The dependent API failed with Status code:" + response.StatusCode,
                //    ReasonPhrase = response.ReasonPhrase
                //};
                throw new HttpRequestException(HttpRequestError.InvalidResponse, errorMessage, ex, response.StatusCode);
            }
            return response;
        }


        private IAuthenticationProvider? GetAuthenticationProvider(AuthenticationKind authenticationKind)
        {
            if (authenticationKind == AuthenticationKind.AzureAdAuthentication)
                return _azureAdAuthenticationProvider;
            else if (authenticationKind == AuthenticationKind.CustomAuthenticationHeaderProvider)
                return _customAuthenticationHeaderProvider;

            return null;
        }

    }
}
