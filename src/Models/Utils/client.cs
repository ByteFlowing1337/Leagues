using System.Net.Http.Headers;
using System.Net.Http;
using System.Net.WebSockets;
using System.Text;

namespace Leagues.Models.Utils;

public static class GetClient
{

    public static HttpClient CreateClient()
    {
        var token = Credential.GetToken();
        var port = Credential.GetPort();

        ArgumentException.ThrowIfNullOrWhiteSpace(token);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(port);

        var handler = new HttpClientHandler
        {
            ServerCertificateCustomValidationCallback =
                HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
        };

        var client = new HttpClient(handler)
        {
            BaseAddress = new Uri($"https://127.0.0.1:{port}/")
        };

        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
            "Basic",
            Convert.ToBase64String(Encoding.ASCII.GetBytes($"riot:{token}")));

        return client;
    }

    public static async Task<WebSocket> CreateWebSocketAsync()
    {
        var token = Credential.GetToken();
        var port = Credential.GetPort();

        ArgumentException.ThrowIfNullOrWhiteSpace(token);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(port);

        var clientWebSocket = new ClientWebSocket();
        clientWebSocket.Options.RemoteCertificateValidationCallback =
            (sender, certificate, chain, sslPolicyErrors) => true;

        var credentials = Convert.ToBase64String(Encoding.ASCII.GetBytes($"riot:{token}"));
        clientWebSocket.Options.SetRequestHeader("Authorization", $"Basic {credentials}");
        var uri = new Uri($"wss://127.0.0.1:{port}/");

        await clientWebSocket.ConnectAsync(uri, CancellationToken.None);

        return clientWebSocket;
    }
}
