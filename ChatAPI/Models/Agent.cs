namespace ChatAPI.Models
{
    public class Agent
    {
        public string Id { get; set; } = Guid.NewGuid().ToString()[..8];
        public Seniority Seniority { get; set; }
        public TeamType Team { get; set; }
        public int CurrentChats { get; set; }
        public DateTime LastAssigned { get; set; } = DateTime.MinValue;

        public int MaxCapacity => (int)Math.Floor(10 * GetMultiplier());

        private double GetMultiplier() => Seniority switch
        {
            Seniority.Junior => 0.4,
            Seniority.MidLevel => 0.6,
            Seniority.TeamLead => 0.5,
            Seniority.Senior => 0.8,
            _ => 0
        };
    }
}
