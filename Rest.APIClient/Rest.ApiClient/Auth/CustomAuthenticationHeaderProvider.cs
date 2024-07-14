using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rest.ApiClient.Auth
{
    public class CustomAuthenticationHeaderProvider : IAuthenticationProvider
    {
        private readonly string _headerKey;
        private readonly string _token;

        public CustomAuthenticationHeaderProvider(string headerKey, string token)
        {
            _headerKey = headerKey;
            _token = token;
        }
        public async Task<HttpRequestMessage> AcquireAndSetAuthenticationHeaderAsync(HttpRequestMessage httpRequestMessage)
        {
            var token = await this.AcquireTokenAsync();
            httpRequestMessage.Headers.Add(_headerKey, token);

            return httpRequestMessage;
        }

        public virtual async Task<string> AcquireTokenAsync()
        {
            await Task.CompletedTask;
            return _token;
        }

        //public CustomAuthenticationHeaderProvider WithHeaderKey(string headerKey, string token)
        //{
        //    this._headerKey = headerKey;
        //    this._token = token;
        //    return this;
        //}

        //public CustomAuthenticationHeaderProvider WithHeaderKeyFromAppSettings(string headerKey, string appSettingKey)
        //{
        //    this._headerKey = headerKey;
        //    this._token = Environment.GetEnvironmentVariable(appSettingKey);
        //    return this;
        //}
    }
}
