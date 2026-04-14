using System.IO;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;

namespace Leagues.Models.Utils;

public sealed class accepted : IAsyncDisposable
{
    private const string ReadyCheckUri = "/lol-matchmaking/v1/ready-check";
    private ClientWebSocket? socket;
    private CancellationTokenSource? monitorCts;
    private Task? monitorTask;
    private bool lastAccepted;

    public event Action<bool>? AcceptChanged;
    public event Action<string>? MonitorError;

    public bool IsMonitoring =>
        socket is { State: WebSocketState.Open } && monitorTask is { IsCompleted: false };

    public bool IsAccepted => lastAccepted;

    public async Task<bool> StartAsync(CancellationToken cancellationToken = default)
    {
        if (IsMonitoring)
        {
            return true;
        }

        try
        {
            monitorCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            var token = monitorCts.Token;

            socket = (ClientWebSocket)await GetClient.CreateWebSocketAsync();
            await SubscribeAsync(token);
            await RefreshCurrentStateAsync(token);

            monitorTask = Task.Run(() => ReceiveLoopAsync(token), token);
            return true;
        }
        catch (Exception ex)
        {
            MonitorError?.Invoke($"Ready-check websocket start failed: {ex.Message}");
            await StopAsync();
            return false;
        }
    }

    private async Task SubscribeAsync(CancellationToken token)
    {
        if (socket is null)
        {
            return;
        }

        var subscriptionMessage = Encoding.UTF8.GetBytes("[5, \"OnJsonApiEvent_lol-matchmaking_v1_ready-check\"]");
        await socket.SendAsync(new ArraySegment<byte>(subscriptionMessage), WebSocketMessageType.Text, true, token);
    }

    private async Task RefreshCurrentStateAsync(CancellationToken token)
    {
        using var client = GetClient.CreateClient();
        using var response = await client.GetAsync("lol-matchmaking/v1/ready-check", token);

        if (!response.IsSuccessStatusCode)
        {
            UpdateAcceptedState(false);
            return;
        }

        await using var stream = await response.Content.ReadAsStreamAsync(token);
        using var document = await JsonDocument.ParseAsync(stream, cancellationToken: token);

        if (TryExtractAcceptedState(document.RootElement, out var isAccepted))
        {
            UpdateAcceptedState(isAccepted);
            return;
        }

        UpdateAcceptedState(false);
    }

    private async Task ReceiveLoopAsync(CancellationToken token)
    {
        var buffer = new byte[4096];

        try
        {
            while (socket is { State: WebSocketState.Open } && !token.IsCancellationRequested)
            {
                using var stream = new MemoryStream();
                WebSocketReceiveResult result;

                do
                {
                    result = await socket.ReceiveAsync(new ArraySegment<byte>(buffer), token);

                    if (result.MessageType == WebSocketMessageType.Close)
                    {
                        await StopAsync();
                        return;
                    }

                    stream.Write(buffer, 0, result.Count);
                } while (!result.EndOfMessage);

                var payload = Encoding.UTF8.GetString(stream.ToArray());
                if (!TryExtractAcceptedState(payload, out var isAccepted))
                {
                    continue;
                }

                UpdateAcceptedState(isAccepted);
            }
        }
        catch (OperationCanceledException)
        {
        }
        catch (Exception ex)
        {
            MonitorError?.Invoke($"Ready-check websocket receive failed: {ex.Message}");
            await StopAsync();
        }
    }

    private void UpdateAcceptedState(bool isAccepted)
    {
        if (lastAccepted == isAccepted)
        {
            return;
        }

        lastAccepted = isAccepted;
        AcceptChanged?.Invoke(isAccepted);
    }

    private static bool TryExtractAcceptedState(string payload, out bool isAccepted)
    {
        isAccepted = false;

        if (string.IsNullOrWhiteSpace(payload))
        {
            return false;
        }

        try
        {
            using var document = JsonDocument.Parse(payload);
            var root = document.RootElement;

            if (root.ValueKind != JsonValueKind.Array || root.GetArrayLength() < 3)
            {
                return false;
            }

            var eventName = root[1].ValueKind == JsonValueKind.String ? root[1].GetString() : null;
            if (!string.Equals(eventName, "OnJsonApiEvent", StringComparison.OrdinalIgnoreCase) &&
                !string.Equals(eventName, "OnJsonApiEvent_lol-matchmaking_v1_ready-check", StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            return TryExtractAcceptedState(root[2], out isAccepted);
        }
        catch (JsonException)
        {
            return false;
        }
    }

    private static bool TryExtractAcceptedState(JsonElement payload, out bool isAccepted)
    {
        isAccepted = false;

        if (payload.ValueKind == JsonValueKind.String)
        {
            var rawValue = payload.GetString();
            isAccepted = string.Equals(rawValue, "Accepted", StringComparison.OrdinalIgnoreCase);
            return rawValue is not null;
        }

        if (payload.ValueKind != JsonValueKind.Object)
        {
            return false;
        }

        if (payload.TryGetProperty("uri", out var uriElement) &&
            uriElement.ValueKind == JsonValueKind.String &&
            !string.Equals(uriElement.GetString(), ReadyCheckUri, StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        if (payload.TryGetProperty("eventType", out var eventTypeElement) &&
            eventTypeElement.ValueKind == JsonValueKind.String &&
            string.Equals(eventTypeElement.GetString(), "Delete", StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        if (payload.TryGetProperty("data", out var dataElement))
        {
            if (dataElement.ValueKind is JsonValueKind.Null or JsonValueKind.Undefined)
            {
                return true;
            }

            payload = dataElement;
        }

        if (payload.ValueKind == JsonValueKind.String)
        {
            var rawValue = payload.GetString();
            isAccepted = string.Equals(rawValue, "Accepted", StringComparison.OrdinalIgnoreCase);
            return rawValue is not null;
        }

        if (payload.ValueKind != JsonValueKind.Object)
        {
            return false;
        }

        var playerResponse = payload.TryGetProperty("playerResponse", out var playerResponseElement) &&
                             playerResponseElement.ValueKind == JsonValueKind.String
            ? playerResponseElement.GetString()
            : null;

        var state = payload.TryGetProperty("state", out var stateElement) &&
                    stateElement.ValueKind == JsonValueKind.String
            ? stateElement.GetString()
            : null;

        isAccepted =
            string.Equals(playerResponse, "Accepted", StringComparison.OrdinalIgnoreCase) ||
            string.Equals(state, "Accepted", StringComparison.OrdinalIgnoreCase);

        if (!string.IsNullOrWhiteSpace(state) &&
            !string.Equals(state, "InProgress", StringComparison.OrdinalIgnoreCase) &&
            !string.Equals(state, "Accepted", StringComparison.OrdinalIgnoreCase))
        {
            isAccepted = false;
        }

        return true;
    }

    public async Task StopAsync()
    {
        monitorCts?.Cancel();
        UpdateAcceptedState(false);

        if (socket is { State: WebSocketState.Open })
        {
            try
            {
                await socket.CloseAsync(WebSocketCloseStatus.NormalClosure, "stop", CancellationToken.None);
            }
            catch
            {
            }
        }

        socket?.Dispose();
        socket = null;
        monitorCts?.Dispose();
        monitorCts = null;
        monitorTask = null;
    }

    public async ValueTask DisposeAsync()
    {
        await StopAsync();
    }
}
