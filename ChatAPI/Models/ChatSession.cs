namespace ChatAPI.Models
{
    public class ChatSession
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public DateTime QueuedAt { get; set; } = DateTime.UtcNow;
        public DateTime LastPollTime { get; set; } = DateTime.UtcNow;
        public string? AssignedAgentId { get; set; }
        public bool IsActive { get; set; } = true;
    }
}
