using Microsoft.AspNetCore.Components;
using System.Net.Http.Headers;

namespace Aml.AdminPanel.Services
{
    public class AuthMessageHandler : DelegatingHandler
    {
        private readonly TokenStorage _tokens;
        private readonly NavigationManager _navigation;

        public AuthMessageHandler(TokenStorage tokens, NavigationManager navigation)
        {
            _tokens = tokens;
            _navigation = navigation;
        }

        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request, CancellationToken ct)
        {
            var token = await _tokens.GetAsync();

            if(!string.IsNullOrEmpty(token))
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var response = await base.SendAsync(request, ct);
            if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized
                && request.RequestUri?.AbsolutePath.EndsWith("api/auth/login") != true)
            {
                await _tokens.RemoveAsync();
                _navigation.NavigateTo("/login", forceLoad: true);
            }

            return response;
        }
    }
}
