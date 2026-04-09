using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;

namespace Leagues.Helper;

public static class Client
{
    public static HttpClient? GetClient()
    {
        var token = Credential.GetToken();
        var port = Credential.GetPort();

        if (string.IsNullOrWhiteSpace(token) || port <= 0)
        {
            return null;
        }

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
}
