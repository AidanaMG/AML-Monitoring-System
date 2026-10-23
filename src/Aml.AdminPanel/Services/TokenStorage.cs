using Microsoft.JSInterop;

namespace Aml.AdminPanel.Services
{
    public class TokenStorage
    {
        private const string Key = "aml_token";
        private readonly IJSRuntime _js;

        public TokenStorage(IJSRuntime js) => _js = js;

        public async Task<string?> GetAsync()
            => await _js.InvokeAsync<string?>("localStorage.getItem", Key);

        public async Task SetAsync(string token)
            => await _js.InvokeVoidAsync("localStorage.setItem", Key, token);

        public async Task RemoveAsync()
            => await _js.InvokeVoidAsync("localStorage.removeItem", Key);
    }
}
