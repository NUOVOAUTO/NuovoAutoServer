using Rest.ApiClient.Auth;
using Rest.ApiClient.Model;

using System.Security.Authentication;
using System.Text.Json;

namespace Rest.ApiClient
{
    public class ApiClient<T> : IApiClient<T> where T : class
    {
        private readonly HttpClient _httpClient;
        private readonly IAuthenticationProvider _authenticationProvider;

        public ApiClient(HttpClient httpClient, IAuthenticationProvider authenticationProvider)
        {
            _httpClient = httpClient;
            _authenticationProvider = authenticationProvider;
        }

        public async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request)
        {
           // var _authenticationProvider = GetAuthenticationProvider(authenticationKind);

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
    }
}
