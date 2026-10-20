using Aml.AdminPanel.Models;
using System.Net.Http.Json;

namespace Aml.AdminPanel.Services
{
    public class AuditLogsApi
    {
        private readonly HttpClient _http;
        public AuditLogsApi(HttpClient http) => _http = http;

        public async Task<AuditListResponse?> GetListAsync(int page, int pageSize)
        {
            var url = $"api/audit?page={page}&pageSize={pageSize}";

            return await _http.GetFromJsonAsync<AuditListResponse>(url);
        }

    }
}
