using System.Net;
using System.Net.Http.Headers;
using System.Net.Http;
using System.Net.WebSockets;
using System.Text;
using static Leagues.Models.Client.Credential;

namespace Leagues.Models.Client;

public static class LcuConnection
{
    public static readonly HttpClient LcuHttpClient = LcuConnectionFactory.CreateHttpClient();

    public static readonly WebSocket
        LcuWebSocket = LcuConnectionFactory.CreateWebSocketAsync().GetAwaiter().GetResult();
}

internal static class LcuConnectionFactory
{
    public static HttpClient CreateHttpClient()
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(Token);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(Port);

        var handler = new HttpClientHandler
        {
            ServerCertificateCustomValidationCallback =
                HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
        };

        var client = new HttpClient(handler)
        {
            BaseAddress = new Uri($"https://127.0.0.1:{Port}/")
        };
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
            "Basic",
            Convert.ToBase64String(Encoding.ASCII.GetBytes($"riot:{Token}")));

        return client;
    }

    public static async Task<WebSocket> CreateWebSocketAsync()
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(Token);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(Port);

        var clientWebSocket = new ClientWebSocket();
        clientWebSocket.Options.RemoteCertificateValidationCallback =
            (sender, certificate, chain, sslPolicyErrors) => true;
        var credentials = Convert.ToBase64String(Encoding.ASCII.GetBytes($"riot:{Token}"));
        clientWebSocket.Options.SetRequestHeader("Authorization", $"Basic {credentials}");
        var uri = new Uri($"wss://127.0.0.1:{Port}/");

        await clientWebSocket.ConnectAsync(uri, CancellationToken.None);
        return clientWebSocket;
    }
}