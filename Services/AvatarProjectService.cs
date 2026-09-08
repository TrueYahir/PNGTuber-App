using System;
using System.IO;
using System.IO.Compression;
using System.Text.Json;
using System.Threading.Tasks;
using PNGTA.Models;

namespace PNGTA.Services;

public class AvatarProjectService
{
    private readonly string _tempExtractPath;

    public AvatarProjectService()
    {
        _tempExtractPath = Path.Combine(Path.GetTempPath(), "PNGTA_Workspace");
    }

    public async Task ExportProjectAsync(CharacterConfig config, string destinationPath)
    {
        string stagingDir = Path.Combine(Path.GetTempPath(), "PNGTA_Staging_" + Guid.NewGuid());
        Directory.CreateDirectory(stagingDir);

        var exportConfig = new CharacterConfig { AudioThreshold = config.AudioThreshold };

        foreach (var state in config.States)
        {
            if (state.Value.Frames.Count > 0 && File.Exists(state.Value.Frames[0].ImagePath))
            {
                string originalPath = state.Value.Frames[0].ImagePath;
                string extension = Path.GetExtension(originalPath);
                string newFileName = $"{state.Key}{extension}";
                string destFile = Path.Combine(stagingDir, newFileName);

                File.Copy(originalPath, destFile, true);

                exportConfig.States[state.Key] = new CharacterState
                {
                    Name = state.Key,
                    Frames = { new SpriteFrame { ImagePath = newFileName } }
                };
            }
        }

        string jsonPath = Path.Combine(stagingDir, "config.json");
        string jsonContent = JsonSerializer.Serialize(exportConfig, new JsonSerializerOptions { WriteIndented = true });
        await File.WriteAllTextAsync(jsonPath, jsonContent);

        if (File.Exists(destinationPath))
        {
            File.Delete(destinationPath);
        }

        ZipFile.CreateFromDirectory(stagingDir, destinationPath, CompressionLevel.Optimal, false);
        Directory.Delete(stagingDir, true);
    }

    public async Task<CharacterConfig> ImportProjectAsync(string sourcePath)
    {
        if (Directory.Exists(_tempExtractPath))
        {
            Directory.Delete(_tempExtractPath, true);
        }
        
        Directory.CreateDirectory(_tempExtractPath);
        ZipFile.ExtractToDirectory(sourcePath, _tempExtractPath);

        string jsonPath = Path.Combine(_tempExtractPath, "config.json");
        if (!File.Exists(jsonPath))
        {
            return new CharacterConfig();
        }

        string jsonContent = await File.ReadAllTextAsync(jsonPath);
        var config = JsonSerializer.Deserialize<CharacterConfig>(jsonContent) ?? new CharacterConfig();

        foreach (var state in config.States)
        {
            if (state.Value.Frames.Count > 0)
            {
                string localFile = state.Value.Frames[0].ImagePath;
                state.Value.Frames[0].ImagePath = Path.Combine(_tempExtractPath, localFile);
            }
        }

        return config;
    }
}