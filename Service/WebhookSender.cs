using System.Text.Json;
using WebSocketGateWay.Core.Interfaces;

namespace WebSocketGateWay.Service
{
    public class WebhookSender : IWebhookSender
    {
        private readonly HttpClient _client;
        private readonly IConfiguration _conf;

        public WebhookSender(HttpClient http, IConfiguration conf)
        {
            _client = http;
            _conf = conf;
        }

        public async Task SendAsync(string message, CancellationToken ct = default)
        {
            var url = _conf["Webhook:Url"];
            if (string.IsNullOrWhiteSpace(url))
                throw new InvalidOperationException("Missing Webhook:Url");

            await _client.PostAsJsonAsync(url,
                new
                {
                    response = JsonSerializer.Deserialize<object>(message),
                    receivedAt = DateTimeOffset.UtcNow
                }
            , ct);
        }
    }
}
