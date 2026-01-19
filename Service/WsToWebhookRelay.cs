using WebSocketGateWay.Core.Interfaces;

namespace WebSocketGateWay.Service
{
    public class WsToWebhookRelay : IHostedService
    {
        private readonly IWebSocketService _ws;
        private readonly IWebhookSender _webhook;

        public WsToWebhookRelay(IWebSocketService ws, IWebhookSender webhook)
        {
            _ws = ws;
            _webhook = webhook;
        }

        public Task StartAsync(CancellationToken ct)
        {
            _ws.OnMessageReceived += msg => _webhook.SendAsync(msg, ct);
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken ct) => Task.CompletedTask;
    }
}
