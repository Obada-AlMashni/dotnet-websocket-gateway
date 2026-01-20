namespace WebSocketGateWay.Core.Interfaces
{
    public interface IWebSocketService
    {
        bool IsConnected { get; }
        event Func<string, Task>? OnMessageReceived;
        Task SendAsync(string message, CancellationToken ct = default);
    }
}
