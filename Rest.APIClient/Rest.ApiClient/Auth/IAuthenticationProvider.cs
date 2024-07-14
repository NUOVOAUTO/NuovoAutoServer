using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rest.ApiClient.Auth
{
    /// <summary>
    /// IAuthenticationProvider can be implemented for different types of authentication mechanisms.
    /// </summary>
    public interface IAuthenticationProvider
    {
        Task<string> AcquireTokenAsync();

        /// <summary>
        /// AcquireAndSetAuthenticationHeaderAsync will acquire the authentication token and set it to the request header based on the respective authentication provider.
        /// </summary>
        /// <param name="httpRequestMessage"></param>
        /// <returns></returns>
        Task<HttpRequestMessage> AcquireAndSetAuthenticationHeaderAsync(HttpRequestMessage httpRequestMessage);
    }
}
