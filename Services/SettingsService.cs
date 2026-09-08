using System.IO;
using System.Text.Json;
using PNGTA.Models;

namespace PNGTA.Services;

public class SettingsService
{
    private const string FilePath = "character_config.json";

    public CharacterConfig LoadConfig()
    {
        if (!File.Exists(FilePath))
            return new CharacterConfig();

        var json = File.ReadAllText(FilePath);
        return JsonSerializer.Deserialize<CharacterConfig>(json) ?? new CharacterConfig();
    }

    public void SaveConfig(CharacterConfig config)
    {
        var options = new JsonSerializerOptions { WriteIndented = true };
        var json = JsonSerializer.Serialize(config, options);
        File.WriteAllText(FilePath, json);
    }
}