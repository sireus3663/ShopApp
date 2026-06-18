using System.Text.Json;
using System.Text.Json.Nodes;

namespace ShopProject.Services;

public class AppConfigService
{
    private readonly string _configPath;

    public AppConfigService(string configPath = "AppConfig.json")
    {
        _configPath = configPath;
    }


    public string GetConnectionString()
    {
        var root = Read();
        return root["ConnectionStrings"]?.ToString() ?? string.Empty;
    }

    public void UpdateDbPassword(string newPassword)
    {
        var conn = GetConnectionString();
        var parts = conn.Split(';');
        for (int i = 0; i < parts.Length; i++)
        {
            if (parts[i].TrimStart().StartsWith("Password=", StringComparison.OrdinalIgnoreCase))
                parts[i] = $"Password={newPassword}";
        }
        var root = Read();
        root["ConnectionStrings"] = string.Join(';', parts);
        Write(root);
    }

    public void UpdateConnectionString(string host, string port, string database, string username, string password)
    {
        var root = Read();
        root["ConnectionStrings"] = $"Host={host};Port={port};Database={database};Username={username};Password={password}";
        Write(root);
    }


    private JsonObject Read()
    {
        if (!File.Exists(_configPath))
            throw new FileNotFoundException($"Файл конфигурации не найден: {_configPath}");
        var json = File.ReadAllText(_configPath);
        return JsonNode.Parse(json)?.AsObject() ?? new JsonObject();
    }

    private void Write(JsonObject root)
    {
        var options = new JsonSerializerOptions { WriteIndented = true };
        File.WriteAllText(_configPath, root.ToJsonString(options));
    }
}
