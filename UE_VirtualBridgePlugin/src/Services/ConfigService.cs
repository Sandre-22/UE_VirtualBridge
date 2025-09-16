using System.IO;
using System.Text.Json;

public class ConfigService
{
    public string UnrealEndpoint { get; private set; } = "http://127.0.0.1:30010"; // fallback

    public ConfigService(string configFile = "config.json")
    {

        
        var configText = File.ReadAllText(configFile);
        using var doc = JsonDocument.Parse(configText);
        UnrealEndpoint = doc.RootElement.GetProperty("UnrealEndpoint").GetString()
                          ?? UnrealEndpoint;
        
    }
}
