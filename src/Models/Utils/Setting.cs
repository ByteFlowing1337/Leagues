using System.IO;
using System.Text.Json;

namespace Leagues.Models.Utils;

public static class Setting
{
    private static readonly string AppDataFolder = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        "Leagues"
    );

    // Create the setting.json under the %APPDATA%
    private static readonly string SettingPath = Path.Combine(AppDataFolder, "settings.json");

    private static readonly JsonSerializerOptions Option = new() { WriteIndented = true };

    public class AppConfig
    {
        public bool AutoAccept { get; set; }
    }

    /// <summary>
    /// If Config is changed, UpdateSetting() should be explicitly called asynchronously
    /// to save the changes to the settings.json file.
    /// </summary>
    public static AppConfig? Config { get; set; } = LoadSetting();

    private static AppConfig? LoadSetting()
    {
        // If file does not exist, create a empty file then return a new AppConfig instance.
        if (!File.Exists(SettingPath))
        {
            return new AppConfig();
        }

        // If the file exists, attempt to read and deserialize it.
        // If deserialization fails, return a new AppConfig instance.
        try
        {
            return JsonSerializer.Deserialize<AppConfig>(File.ReadAllText(SettingPath), Option);
        }
        catch (Exception)
        {
            return new AppConfig();
        }
    }

    /// <summary>
    /// Should be explicitly called whenever the Config is changed.
    /// </summary>
    public static async Task UpdateSetting()
    {
        var updatedContent = JsonSerializer.Serialize(Config, Option);
        await Task.Run(() =>
        {
            Directory.CreateDirectory(AppDataFolder);
            File.WriteAllText(SettingPath, updatedContent);
        });
    }
}