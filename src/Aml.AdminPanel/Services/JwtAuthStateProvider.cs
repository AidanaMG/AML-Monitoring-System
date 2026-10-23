using Microsoft.AspNetCore.Components.Authorization;
using System.Security.Claims;
using System.Text;
using System.Text.Json;

namespace Aml.AdminPanel.Services
{
    public class JwtAuthStateProvider : AuthenticationStateProvider
    {
        private readonly TokenStorage _tokens;
        public JwtAuthStateProvider(TokenStorage tokens) => _tokens = tokens;

        public override async Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            try
            {
                var token = await _tokens.GetAsync();

                if (string.IsNullOrEmpty(token))
                    return new AuthenticationState(new ClaimsPrincipal());

                var claims = ParseClaims(token).ToList();

                var exp = claims.FirstOrDefault(c => c.Type == "exp")?.Value;
                if(exp is not null && long.TryParse(exp, out var seconds))
                {
                    var expiry = DateTimeOffset.FromUnixTimeSeconds(seconds);
                    if(expiry < DateTimeOffset.UtcNow)
                    {
                        await _tokens.RemoveAsync();
                        return new AuthenticationState(new ClaimsPrincipal());
                    }
                }

                var identity = new ClaimsIdentity(claims, "jwt", ClaimTypes.Name, ClaimTypes.Role);

                return new AuthenticationState(new ClaimsPrincipal(identity));

            }
            catch
            {
                await _tokens.RemoveAsync();
                return new AuthenticationState(new ClaimsPrincipal());
            }

            
        }
            

        public async Task NotifyUserAuthenticationAsync()
        {
            var state = await GetAuthenticationStateAsync();
            NotifyAuthenticationStateChanged(Task.FromResult(state));
        }


        public void NotifyUserLogout()
        {
            var anonymous = new AuthenticationState(new ClaimsPrincipal());
            NotifyAuthenticationStateChanged(Task.FromResult(anonymous));
        }


        private static IEnumerable<Claim> ParseClaims(string token) 
        {
            var payLoad = token.Split('.')[1];

            switch(payLoad.Length % 4)
            {
                case 2: payLoad += "=="; 
                    break;
                case 3: payLoad += "=";
                    break;
            }

            var json = Encoding.UTF8.GetString(Convert.FromBase64String(payLoad));
            var pairs = JsonSerializer.Deserialize<Dictionary<string, object>>(json)!;

            return pairs.Select(p => new Claim(p.Key, p.Value.ToString()!));
        }
    }
}