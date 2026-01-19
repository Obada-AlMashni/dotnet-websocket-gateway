using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using WebSocketGateWay.Core.Interfaces;

namespace WebSocketGateWay.Service
{
    public class WsToWebhookRelay : IHostedService
    {
        private readonly IWebSocketService _ws;
        private readonly IWebhookSender _webhook;
        private readonly ILogger<WsToWebhookRelay> _logger;

        public WsToWebhookRelay(IWebSocketService ws, IWebhookSender webhook, ILogger<WsToWebhookRelay> logger)
        {
            _ws = ws;
            _webhook = webhook;
            _logger = logger;
        }

        public Task StartAsync(CancellationToken ct)
        {
            _ws.OnMessageReceived += HandleMessageAsync;
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken ct)
        {
            _ws.OnMessageReceived -= HandleMessageAsync;
            return Task.CompletedTask;
        }

        private async Task HandleMessageAsync(string msg)
        {
            try
            {
                await _webhook.SendAsync(msg);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to forward WS message to webhook");
            }
        }
    }
}
