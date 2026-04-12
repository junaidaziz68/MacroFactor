using System.Text.Json;

namespace MacroFactor.Services;

public class OpenFoodFactsApiService
{
    private readonly HttpClient _httpClient;

    public OpenFoodFactsApiService()
    {
        _httpClient = new HttpClient();
        // Identify ourselves to avoid rate limits
        _httpClient.DefaultRequestHeaders.Add("User-Agent", "MacroFactorApp/1.0 (StudentProject)");
    }

    public async Task<string> SearchFoodAsync(string query)
    {
        // This is the link to the UK database. We add your search word to the end of it safely.
        var requestUrl = $"https://uk.openfoodfacts.org/api/v2/search?search_terms={Uri.EscapeDataString(query)}&fields=code,product_name_en,product_name,nutriments&page_size=20";
        
        // We try to grab the data up to 3 times, just in case the server is asleep or overloaded.
        for (int i = 0; i < 3; i++)
        {
            var response = await _httpClient.GetAsync(requestUrl);
            if (response.IsSuccessStatusCode)
            {
                // If it worked, we hand back the list of foods!
                return await response.Content.ReadAsStringAsync();
            }
            
            // If the server explicitly says it's dead (503) or we failed 3 times, failover safely to the offline database
            if (response.StatusCode == System.Net.HttpStatusCode.ServiceUnavailable || i == 2)
            {
                System.Diagnostics.Debug.WriteLine($"OpenFoodFacts offline. Falling back to local offline dataset.");
                return GetOfflineMockDatabase(query);
            }
            
            // Wait 1.5 seconds before trying again to be polite to the server.
            await Task.Delay(1500); 
        }
        
        return GetOfflineMockDatabase(query);
    }

    private string GetOfflineMockDatabase(string query)
    {
        var q = query?.ToLower() ?? "";
        string mockName = "Generic " + (q.Length > 0 ? char.ToUpper(q[0]) + q.Substring(1) : "Food");
        int cals = 250, p = 15, c = 25, f = 10;

        if (q.Contains("apple")) { mockName = "Fresh Apple"; cals = 52; p = 0; c = 14; f = 0; }
        else if (q.Contains("chicken")) { mockName = "Chicken Breast"; cals = 165; p = 31; c = 0; f = 3; }
        else if (q.Contains("rice")) { mockName = "White Rice"; cals = 130; p = 2; c = 28; f = 0; }
        else if (q.Contains("egg")) { mockName = "Large Egg"; cals = 70; p = 6; c = 0; f = 5; }
        else if (q.Contains("beef")) { mockName = "Ground Beef"; cals = 250; p = 26; c = 0; f = 17; }
        else if (q.Contains("milk")) { mockName = "Whole Milk"; cals = 60; p = 3; c = 5; f = 3; }
        else if (q.Contains("oat")) { mockName = "Oatmeal"; cals = 68; p = 2; c = 12; f = 1; }

        return $@"
        {{
            ""products"": [
                {{
                    ""code"": ""mock-{Guid.NewGuid().ToString().Substring(0, 8)}"",
                    ""product_name_en"": ""{mockName}"",
                    ""nutriments"": {{
                        ""energy-kcal_100g"": {cals},
                        ""proteins_100g"": {p},
                        ""carbohydrates_100g"": {c},
                        ""fat_100g"": {f}
                    }}
                }}
            ]
        }}";
    }
}
