using Supabase;

namespace MacroFactor.Services;

public class SupabaseService
{
    private readonly Client _client;

    public SupabaseService()
    {
        var url = "https://igvbchrbijitokkjwkwd.supabase.co";
        var key = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJpc3MiOiJzdXBhYmFzZSIsInJlZiI6ImlndmJjaHJiaWppdG9ra2p3a3dkIiwicm9sZSI6ImFub24iLCJpYXQiOjE3NzE1MDkwNTcsImV4cCI6MjA4NzA4NTA1N30.r2ay2BzKudNkeWRIzJJV2rJ_W-tEe_-YHpm596e0kBA";

        _client = new Client(url, key);
    }

    public async Task InitializeAsync()
    {
        await _client.InitializeAsync();
    }

    public Client GetClient() => _client;
}