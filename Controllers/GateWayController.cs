using Microsoft.AspNetCore.Mvc;
using WebSocketGateWay.Core.Dtos;
using WebSocketGateWay.Core.Interfaces;

namespace WebSocketGateWay.Controllers
{
    [ApiController]
    [Route("api/gateway")]
    public class GateWayController : ControllerBase
    {
        private readonly IWebSocketService _ws;

        public GateWayController(IWebSocketService ws) => _ws = ws;

        [HttpPost("send")]
        public async Task<IActionResult> Send([FromBody] SendDto dto, CancellationToken ct)
        {
            await _ws.SendAsync(dto.Message, ct);
            return Ok(new { sent = true });
        }
    }
}