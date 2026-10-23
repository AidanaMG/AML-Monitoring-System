using Aml.AdminPanel.Models;
using System.Net.Http.Json;

namespace Aml.AdminPanel.Services
{
    public class RulesApi
    {
        private readonly HttpClient _http;
        public RulesApi(HttpClient http) => _http = http;

        public async Task<RuleListItem[]?> GetListAsync()
            => await _http.GetFromJsonAsync<RuleListItem[]>("api/rules");

        public async Task<HttpResponseMessage> UpdateAsync(int id, UpdateRuleRequest request)
            => await _http.PatchAsJsonAsync($"api/rules/{id}", request);
    }
}
