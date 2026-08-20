using System.IO;
using System.Windows.Media.Imaging;
using Leagues.Models.Client;

namespace Leagues.Models.Mapper;

public static class ChampionMapper
{
    public static async Task<BitmapImage> ChampionIdToImage(int championId)
    {
        var avatarUrl = LcuEndPoint.ChampionAvatar(championId);
        var bytes = await LcuConnection.LcuHttpClient.GetByteArrayAsync(avatarUrl);
        using var ms = new MemoryStream(bytes);
        var bitmap = new BitmapImage();
        bitmap.BeginInit();
        bitmap.CacheOption = BitmapCacheOption.OnLoad; // Load fully into memory
        bitmap.StreamSource = ms;
        bitmap.EndInit();
        bitmap.Freeze(); // Freezes thread ownership so WPF UI can render it safely
        return bitmap;
    }
}