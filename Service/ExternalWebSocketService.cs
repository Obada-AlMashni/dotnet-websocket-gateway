using System.Net.WebSockets;
using System.Text;
using WebSocketGateWay.Core.Interfaces;

namespace WebSocketGateWay.Service;

public sealed class ExternalWebSocketService : BackgroundService, IWebSocketService
{
    private readonly IConfiguration _cfg;
    private readonly ILogger<ExternalWebSocketService> _logger;
    private readonly IHttpClientFactory _httpFactory;
    private ClientWebSocket? _ws;
    private readonly SemaphoreSlim _sendLock = new(1, 1);
    public bool IsConnected => _ws?.State == WebSocketState.Open;
    public event Func<string, Task>? OnMessageReceived;

    public ExternalWebSocketService(
        IConfiguration cfg,
        ILogger<ExternalWebSocketService> logger,
        IHttpClientFactory httpFactory)
    {
        _cfg = cfg;
        _logger = logger;
        _httpFactory = httpFactory;
    }

    public async Task SendAsync(string message, CancellationToken ct = default)
    {
        var ws = _ws;
        if (ws is null || ws.State != WebSocketState.Open)
            throw new InvalidOperationException("WebSocket is not connected.");

        var bytes = Encoding.UTF8.GetBytes(message);

        await _sendLock.WaitAsync(ct);
        try
        {
            await ws.SendAsync(bytes, WebSocketMessageType.Text, endOfMessage: true, cancellationToken: ct);
        }
        finally
        {
            _sendLock.Release();
        }
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var wsUrl = _cfg["ExternalWs:Url"];
        if (string.IsNullOrWhiteSpace(wsUrl))
            throw new InvalidOperationException("Missing config ExternalWs:Url");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                _ws = new ClientWebSocket();

                // Optional: if your WS requires headers
                var apiKey = _cfg["ExternalWs:ApiKey"];
                if (!string.IsNullOrWhiteSpace(apiKey))
                    _ws.Options.SetRequestHeader("X-Api-Key", apiKey);

                _logger.LogInformation("Connecting to external WS: {Url}", wsUrl);
                await _ws.ConnectAsync(new Uri(wsUrl), stoppingToken);
                _logger.LogInformation("External WS connected.");

                await ReceiveLoopAsync(_ws, stoppingToken);
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "External WS error. Reconnecting...");
            }

            await Task.Delay(TimeSpan.FromSeconds(2), stoppingToken);
        }
    }

    private async Task ReceiveLoopAsync(ClientWebSocket ws, CancellationToken ct)
    {
        var buffer = new byte[8 * 1024];

        while (ws.State == WebSocketState.Open && !ct.IsCancellationRequested)
        {
            var sb = new StringBuilder();
            WebSocketReceiveResult result;

            do
            {
                result = await ws.ReceiveAsync(buffer, ct);

                if (result.MessageType == WebSocketMessageType.Close)
                {
                    _logger.LogWarning("WS closed by server: {Status} {Desc}",
                        ws.CloseStatus, ws.CloseStatusDescription);

                    try
                    {
                        await ws.CloseAsync(WebSocketCloseStatus.NormalClosure, "closing", ct);
                    }
                    catch { /* ignore */ }

                    return;
                }

                sb.Append(Encoding.UTF8.GetString(buffer, 0, result.Count));
            }
            while (!result.EndOfMessage);

            var msg = sb.ToString();

            if (OnMessageReceived is not null)
            {
                try { await OnMessageReceived.Invoke(msg); }
                catch (Exception ex) { _logger.LogError(ex, "OnMessageReceived handler failed"); }
            }

            await ForwardToWebhookAsync(msg, ct);
        }
    }

    private async Task ForwardToWebhookAsync(string msg, CancellationToken ct)
    {
        var webhookUrl = _cfg["Webhook:Url"];
        if (string.IsNullOrWhiteSpace(webhookUrl))
        {
            _logger.LogWarning("Webhook:Url not configured, skipping forward.");
            return;
        }

        var client = _httpFactory.CreateClient();

        //var payload = new
        //{
        //    message = msg,
        //    receivedAt = DateTimeOffset.UtcNow
        //};

        try
        {
            using var res = await client.PostAsJsonAsync(webhookUrl, msg, ct);

            if (!res.IsSuccessStatusCode)
            {
                var body = await res.Content.ReadAsStringAsync(ct);
                _logger.LogWarning("Webhook forward failed: {Status} {Body}",
                    (int)res.StatusCode, body);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Webhook forward threw exception");
        }
    }
}
