using Rest.ApiClient.Auth;

namespace Rest.ApiClient
{
    /// <summary>
    /// The API Client interface used to send external api requests with authentication provider. 
    /// <code>
    /// The general user case: 
    ///     Inject the IApiClient and  authentication provider of type AuthenticationProviders as a dependency.
    /// </code>
    /// </summary>
    public interface IApiClient
    {
        /// <summary>
        /// Executes the API request and retuns the response message.
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, AuthenticationKind authenticationKind);

    }
}