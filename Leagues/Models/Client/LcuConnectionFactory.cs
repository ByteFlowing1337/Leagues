using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.WebSockets;
using System.Text;

namespace Leagues.Models.Client;

public static class LcuConnection
{
    private static readonly object SyncRoot = new();
    private static HttpClient? _httpClient;
    private static ClientCredentials? _activeCredentials;

    public static HttpClient LcuHttpClient
    {
        get
        {
            if (!Credential.TryGetCredentials(out var credentials))
                throw new InvalidOperationException("League client credentials are unavailable.");

            lock (SyncRoot)
            {
                if (_httpClient is not null && _activeCredentials is { } active &&
                    active.Port == credentials.Port && active.Token == credentials.Token)
                    return _httpClient;

                _httpClient?.Dispose();
                _httpClient = LcuConnectionFactory.CreateHttpClient(credentials);
                _activeCredentials = credentials;
                return _httpClient;
            }
        }
    }
}

internal static class LcuConnectionFactory
{
    public static HttpClient CreateHttpClient(ClientCredentials credentials)
    {
        var handler = new HttpClientHandler
        {
            ServerCertificateCustomValidationCallback =
                HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
        };

        var client = new HttpClient(handler) { BaseAddress = new Uri($"https://127.0.0.1:{credentials.Port}/") };
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
            "Basic",
            Convert.ToBase64String(Encoding.ASCII.GetBytes($"riot:{credentials.Token}")));

        return client;
    }

    public static async Task<WebSocket> CreateWebSocketAsync()
    {
        if (!Credential.TryGetCredentials(out var credentials))
            throw new InvalidOperationException("League client credentials are unavailable.");

        var clientWebSocket = new ClientWebSocket();
        ServicePointManager.ServerCertificateValidationCallback =
            (_, _, _, _) => true;
        var authorization = Convert.ToBase64String(Encoding.ASCII.GetBytes($"riot:{credentials.Token}"));
        clientWebSocket.Options.SetRequestHeader("Authorization", $"Basic {authorization}");
        var uri = new Uri($"wss://127.0.0.1:{credentials.Port}/");

        await clientWebSocket.ConnectAsync(uri, CancellationToken.None);
        return clientWebSocket;
    }
}