using System.Net.WebSockets;
using System.IO;
using System.Text;
using System.Text.Json;

namespace Leagues.Models.Client;

public sealed class Phase : IAsyncDisposable
{
    private ClientWebSocket? socket;
    private CancellationTokenSource? monitorCts;
    private Task? monitorTask;
    private string? lastPhase;

    public event EventHandler<string>? PhaseChanged;
    public event EventHandler<string>? MonitorError;

    public bool IsMonitoring =>
        socket is { State: WebSocketState.Open } && monitorTask is { IsCompleted: false };

    public string? CurrentPhase => lastPhase;

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

            socket = (ClientWebSocket)await LcuConnectionFactory.CreateWebSocketAsync();
            await SubscribeAsync(token);

            monitorTask = Task.Run(() => ReceiveLoopAsync(token), token);
            return true;
        }
        catch (Exception ex)
        {
            MonitorError?.Invoke(this, $"Phase websocket start failed: {ex.Message}");
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

        var subscriptionMessage = Encoding.UTF8.GetBytes("[5, \"OnJsonApiEvent_lol-gameflow_v1_gameflow-phase\"]");
        await socket.SendAsync(new ArraySegment<byte>(subscriptionMessage), WebSocketMessageType.Text, true, token);
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
                var phase = TryExtractPhase(payload);
                if (string.IsNullOrWhiteSpace(phase))
                {
                    continue;
                }

                if (!string.Equals(lastPhase, phase, StringComparison.OrdinalIgnoreCase))
                {
                    lastPhase = phase;
                    PhaseChanged?.Invoke(this, phase);
                }
            }
        }
        catch (OperationCanceledException)
        {
        }
        catch (Exception ex)
        {
            MonitorError?.Invoke(this, $"Phase websocket receive failed: {ex.Message}");
            await StopAsync();
        }
    }

    private static string? TryExtractPhase(string payload)
    {
        if (string.IsNullOrWhiteSpace(payload))
        {
            return null;
        }

        try
        {
            using var doc = JsonDocument.Parse(payload);
            var root = doc.RootElement;

            if (root.ValueKind != JsonValueKind.Array || root.GetArrayLength() < 3)
            {
                return null;
            }

            var eventName = root[1].ValueKind == JsonValueKind.String ? root[1].GetString() : null;
            if (!string.Equals(eventName, "OnJsonApiEvent", StringComparison.OrdinalIgnoreCase) &&
                !string.Equals(eventName, "OnJsonApiEvent_lol-gameflow_v1_gameflow-phase",
                    StringComparison.OrdinalIgnoreCase))
            {
                return null;
            }

            var eventBody = root[2];
            if (eventBody.ValueKind == JsonValueKind.String)
            {
                return eventBody.GetString();
            }

            if (eventBody.ValueKind != JsonValueKind.Object)
            {
                return null;
            }

            if (eventBody.TryGetProperty("uri", out var uriElement) && uriElement.ValueKind == JsonValueKind.String)
            {
                var uri = uriElement.GetString();
                if (!string.Equals(uri, "/lol-gameflow/v1/gameflow-phase", StringComparison.OrdinalIgnoreCase))
                {
                    return null;
                }
            }

            if (eventBody.TryGetProperty("data", out var dataElement) && dataElement.ValueKind == JsonValueKind.String)
            {
                return dataElement.GetString();
            }
        }
        catch (JsonException)
        {
        }

        return null;
    }

    public async Task StopAsync()
    {
        monitorCts?.Cancel();

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
        lastPhase = null;
        monitorTask = null;
    }

    public async ValueTask DisposeAsync()
    {
        await StopAsync();
    }
}