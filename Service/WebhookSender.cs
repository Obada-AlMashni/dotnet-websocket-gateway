using WebSocketGateWay.Core.Interfaces;

namespace WebSocketGateWay.Service
{
    public class WebhookSender : IWebhookSender
    {
        private readonly HttpClient _http;
        private readonly IConfiguration _cfg;
        private readonly ILogger<WebhookSender> _logger;

        public WebhookSender(HttpClient http, IConfiguration cfg, ILogger<WebhookSender> logger)
        {
            _http = http;
            _cfg = cfg;
            _logger = logger;
        }

        public async Task SendAsync(string rawMessage, CancellationToken ct = default)
        {
            var url = _cfg["Webhook:Url"];
            if (string.IsNullOrWhiteSpace(url))
                throw new InvalidOperationException("Missing config: Webhook:Url");

            //var payload = new
            //{
            //    message = rawMessage,
            //    receivedAt = DateTimeOffset.UtcNow
            //};

            var res = await _http.PostAsJsonAsync(url, rawMessage, ct);

            if (!res.IsSuccessStatusCode)
            {
                var body = await res.Content.ReadAsStringAsync(ct);
                _logger.LogWarning("Webhook failed: {Status} {Body}", (int)res.StatusCode, body);
            }
        }
    }
}
