using System.IO;
using System.Text.Json;
using PNGTA.Models;

namespace PNGTA.Services;

public class AppSettingsService
{
    private const string SettingsPath = "appsettings.json";

    public AppSettings LoadSettings()
    {
        if (!File.Exists(SettingsPath))
        {
            return new AppSettings();
        }

        try
        {
            var json = File.ReadAllText(SettingsPath);
            return JsonSerializer.Deserialize<AppSettings>(json) ?? new AppSettings();
        }
        catch
        {
            return new AppSettings();
        }
    }

    public void SaveSettings(AppSettings settings)
    {
        var options = new JsonSerializerOptions { WriteIndented = true };
        var json = JsonSerializer.Serialize(settings, options);
        File.WriteAllText(SettingsPath, json);
    }
}