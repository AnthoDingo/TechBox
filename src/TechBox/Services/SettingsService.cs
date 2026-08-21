using System;
using System.IO;
using System.Text.Json;
using TechBox.Models;

namespace TechBox.Services;

/// <summary>
/// Persists <see cref="AppSettings"/> as JSON under the current user's AppData folder.
/// </summary>
public sealed class SettingsService : ISettingsService
{
    private static readonly string SettingsDirectory = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "TechBox");

    private static readonly string SettingsFilePath = Path.Combine(SettingsDirectory, "settings.json");

    private static readonly JsonSerializerOptions SerializerOptions = new() { WriteIndented = true };

    public AppSettings Load()
    {
        if (!File.Exists(SettingsFilePath))
        {
            return new AppSettings();
        }

        try
        {
            var json = File.ReadAllText(SettingsFilePath);
            return JsonSerializer.Deserialize<AppSettings>(json, SerializerOptions) ?? new AppSettings();
        }
        catch (Exception ex) when (ex is IOException or JsonException or UnauthorizedAccessException)
        {
            return new AppSettings();
        }
    }

    public void Save(AppSettings settings)
    {
        Directory.CreateDirectory(SettingsDirectory);
        var json = JsonSerializer.Serialize(settings, SerializerOptions);
        File.WriteAllText(SettingsFilePath, json);
    }
}
