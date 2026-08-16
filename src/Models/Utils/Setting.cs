using System.IO;
using System.Text.Json;

namespace Leagues.Models.Utils;

public static class Setting
{
    private static readonly string AppDataFolder = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        "Leagues"
    );

    private static readonly string SettingPath = Path.Combine(AppDataFolder, "settings.json");

    private static readonly JsonSerializerOptions Option = new() { WriteIndented = true };

    public class AppConfig
    {
        public bool AutoAccept { get; set; }
    }

    public static AppConfig? Config { get; set; } = LoadSetting();

    private static AppConfig? LoadSetting()
    {
        if (!File.Exists(SettingPath))
        {
            return new AppConfig();
        }

        try
        {
            return JsonSerializer.Deserialize<AppConfig>(File.ReadAllText(SettingPath), Option);
        }
        catch (Exception)
        {
            return new AppConfig();
        }
    }

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