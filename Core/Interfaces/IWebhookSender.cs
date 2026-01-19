namespace WebSocketGateWay.Core.Interfaces
{
    public interface IWebhookSender
    {
        Task SendAsync(string rawMessage, CancellationToken ct = default);
    }
}
