using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace MacroFactor.Services;

public class FatSecretApiService
{
    // The user MUST fill these in after creating a developer account
    private const string ClientId = "7df3746fa0334f1fb947113bb2839b75";
    private const string ClientSecret = "16184cce9a224094ae6e66227fe1204d";
    
    private readonly HttpClient _httpClient;
    private string _accessToken;
    private DateTime _tokenExpiration;

    public FatSecretApiService()
    {
        _httpClient = new HttpClient();
    }

    private async Task EnsureAuthenticatedAsync()
    {
        if (!string.IsNullOrEmpty(_accessToken) && DateTime.Now < _tokenExpiration)
        {
            return;
        }

        var requestUrl = "https://oauth.fatsecret.com/connect/token";
        var credentials = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{ClientId}:{ClientSecret}"));

        var request = new HttpRequestMessage(HttpMethod.Post, requestUrl);
        request.Headers.Authorization = new AuthenticationHeaderValue("Basic", credentials);

        var content = new FormUrlEncodedContent(new[]
        {
            new KeyValuePair<string, string>("grant_type", "client_credentials"),
            new KeyValuePair<string, string>("scope", "basic")
        });
        
        request.Content = content;

        var response = await _httpClient.SendAsync(request);
        
        if (response.IsSuccessStatusCode)
        {
            var json = await response.Content.ReadAsStringAsync();
            var tokenData = JsonSerializer.Deserialize<JsonElement>(json);
            
            _accessToken = tokenData.GetProperty("access_token").GetString();
            var expiresIn = tokenData.GetProperty("expires_in").GetInt32();
            
            // Give a 1-minute buffer before it actually expires
            _tokenExpiration = DateTime.Now.AddSeconds(expiresIn - 60);
        }
        else
        {
            throw new Exception($"FatSecret Authentication Failed: {response.ReasonPhrase}");
        }
    }

    public async Task<string> SearchFoodAsync(string query)
    {
        await EnsureAuthenticatedAsync();

        var requestUrl = $"https://platform.fatsecret.com/rest/server.api?method=foods.search.v2&search_expression={Uri.EscapeDataString(query)}&format=json";
        
        var request = new HttpRequestMessage(HttpMethod.Get, requestUrl);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _accessToken);

        var response = await _httpClient.SendAsync(request);
        
        if (response.IsSuccessStatusCode)
        {
            return await response.Content.ReadAsStringAsync();
        }

        throw new Exception($"FatSecret Search Failed: {response.ReasonPhrase}");
    }
}
