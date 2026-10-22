using System.Net.WebSockets;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace JoyclubObsBridge.Services;

public sealed class ObsWebSocketClient : IAsyncDisposable
{
    private ClientWebSocket? _socket;

    public async Task<string> ConnectAndGetVersionAsync(string host, int port, string password, CancellationToken cancellationToken = default)
    {
        _socket = new ClientWebSocket();
        await _socket.ConnectAsync(new Uri($"ws://{host}:{port}"), cancellationToken);
        using var hello = JsonDocument.Parse(await ReceiveAsync(cancellationToken));
        if (hello.RootElement.GetProperty("op").GetInt32() != 0) throw new InvalidOperationException("OBS hat kein Hello-Paket gesendet.");

        var identify = new Dictionary<string, object?> { ["rpcVersion"] = 1 };
        if (hello.RootElement.GetProperty("d").TryGetProperty("authentication", out var authentication))
        {
            if (string.IsNullOrEmpty(password)) throw new InvalidOperationException("OBS verlangt ein WebSocket-Passwort.");
            identify["authentication"] = CreateAuthentication(password, authentication.GetProperty("salt").GetString()!, authentication.GetProperty("challenge").GetString()!);
        }
        await SendAsync(1, identify, cancellationToken);
        using var identified = JsonDocument.Parse(await ReceiveAsync(cancellationToken));
        if (identified.RootElement.GetProperty("op").GetInt32() != 2) throw new InvalidOperationException("OBS-WebSocket-Identifikation fehlgeschlagen.");

        const string requestId = "version";
        await SendAsync(6, new { requestType = "GetVersion", requestId }, cancellationToken);
        using var response = JsonDocument.Parse(await ReceiveAsync(cancellationToken));
        var data = response.RootElement.GetProperty("d");
        var status = data.GetProperty("requestStatus").GetProperty("result").GetBoolean();
        if (!status) throw new InvalidOperationException("OBS hat GetVersion abgelehnt.");
        return data.GetProperty("responseData").GetProperty("obsVersion").GetString() ?? "unbekannt";
    }

    internal static string CreateAuthentication(string password, string salt, string challenge)
    {
        var secret = Convert.ToBase64String(SHA256.HashData(Encoding.UTF8.GetBytes(password + salt)));
        return Convert.ToBase64String(SHA256.HashData(Encoding.UTF8.GetBytes(secret + challenge)));
    }

    private async Task SendAsync(int op, object data, CancellationToken cancellationToken)
    {
        var json = JsonSerializer.Serialize(new { op, d = data });
        await _socket!.SendAsync(Encoding.UTF8.GetBytes(json), WebSocketMessageType.Text, true, cancellationToken);
    }

    private async Task<string> ReceiveAsync(CancellationToken cancellationToken)
    {
        var buffer = new byte[8192];
        var builder = new StringBuilder();
        WebSocketReceiveResult result;
        do
        {
            result = await _socket!.ReceiveAsync(buffer, cancellationToken);
            if (result.MessageType == WebSocketMessageType.Close) throw new WebSocketException("OBS-WebSocket wurde geschlossen.");
            builder.Append(Encoding.UTF8.GetString(buffer, 0, result.Count));
        } while (!result.EndOfMessage);
        return builder.ToString();
    }

    public async ValueTask DisposeAsync()
    {
        if (_socket is not null) { _socket.Dispose(); _socket = null; }
        await Task.CompletedTask;
    }
}
