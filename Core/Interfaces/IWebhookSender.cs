namespace WebSocketGateWay.Core.Interfaces
{
    public interface IWebhookSender
    {
        Task SendAsync(string message, CancellationToken ct = default);
    }
}
