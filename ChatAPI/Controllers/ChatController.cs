using ChatAPI.Services;
using Microsoft.AspNetCore.Mvc;

namespace ChatAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ChatController(ChatManager manager) : ControllerBase
    {
        [HttpPost("start")]
        public IActionResult StartChat()
        {
            var (success, sessionId, message) = manager.TryQueueChat();

            if (!success)
                return StatusCode(StatusCodes.Status503ServiceUnavailable, new { Message = message });

            return Ok(new { SessionId = sessionId, Message = message });
        }

        [HttpPost("{sessionId}/poll")]
        public IActionResult PollSession(Guid sessionId)
        {
            if (manager.RecordPoll(sessionId))
                return Ok();

            return NotFound("Session not found or marked inactive.");
        }

        [HttpGet("status")]
        public IActionResult GetStatus()
        {
            return Ok(manager.GetStatus());
        }
    }
}
