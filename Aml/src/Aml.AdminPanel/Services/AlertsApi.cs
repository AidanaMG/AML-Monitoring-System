using Aml.AdminPanel.Models;
using System.Net.Http.Json;

namespace Aml.AdminPanel.Services
{
    public class AlertsApi
    {
        private readonly HttpClient _http;
        public AlertsApi(HttpClient http) => _http = http;

        public async Task<AlertListResponse?> GetListAsync(
            IEnumerable<string> statuses, int page, int pageSize)
        {
            var url = $"api/alerts?page={page}&pageSize={pageSize}";

            foreach(var s in statuses)
            {
                url += $"&status={Uri.EscapeDataString(s)}";
            }
                

            return await _http.GetFromJsonAsync<AlertListResponse>(url);
        }


        public async Task<AlertDetails?> GetByIdAsync(int id)
            => await _http.GetFromJsonAsync<AlertDetails>($"api/alerts/{id}");

        public async Task<HttpResponseMessage> ResolveAsync(int id, string status, string? comment)
            => await _http.PatchAsJsonAsync($"api/alerts/{id}/resolve", new { status, comment });

        public async Task<HttpResponseMessage> TakeToReviewAsync(int id)
            => await _http.PatchAsync($"api/alerts/{id}/take", null);
    }
}
