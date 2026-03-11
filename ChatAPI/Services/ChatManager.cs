using System.Collections.Concurrent;
using ChatAPI.Models;

namespace ChatAPI.Services
{
    public class ChatManager
    {
        private readonly List<Agent> _agents = new();
        public ConcurrentQueue<ChatSession> Queue { get; } = new();
        public ConcurrentDictionary<Guid, ChatSession> ActiveSessions { get; } = new();

        private readonly object _assignmentLock = new();

        public ChatManager()
        {
            // Team A (Office Hours: 08:00 - 16:00)
            _agents.Add(new Agent { Team = TeamType.A, Seniority = Seniority.TeamLead });
            _agents.Add(new Agent { Team = TeamType.A, Seniority = Seniority.MidLevel });
            _agents.Add(new Agent { Team = TeamType.A, Seniority = Seniority.MidLevel });
            _agents.Add(new Agent { Team = TeamType.A, Seniority = Seniority.Junior });

            // Team B (Evening: 16:00 - 00:00)
            _agents.Add(new Agent { Team = TeamType.B, Seniority = Seniority.Senior });
            _agents.Add(new Agent { Team = TeamType.B, Seniority = Seniority.MidLevel });
            _agents.Add(new Agent { Team = TeamType.B, Seniority = Seniority.Junior });
            _agents.Add(new Agent { Team = TeamType.B, Seniority = Seniority.Junior });

            // Team C (Night Shift: 00:00 - 08:00)
            _agents.Add(new Agent { Team = TeamType.C, Seniority = Seniority.MidLevel });
            _agents.Add(new Agent { Team = TeamType.C, Seniority = Seniority.MidLevel });

            // Overflow Team (6x Juniors)
            for (int i = 0; i < 6; i++)
                _agents.Add(new Agent { Team = TeamType.Overflow, Seniority = Seniority.Junior });
        }

        public bool IsOfficeHours => DateTime.UtcNow.Hour >= 8 && DateTime.UtcNow.Hour < 16;

        private static List<TeamType> GetActiveTeams(bool includeOverflow = false)
        {
            var hour = DateTime.Now.Hour;

            if (hour < 8) return [TeamType.C];
            if (hour < 16) return includeOverflow
                ? [TeamType.A, TeamType.Overflow]
                : [TeamType.A];

            return [TeamType.B];
        }

        private List<Agent> GetActiveShiftAgents()
        {
            var teams = GetActiveTeams(includeOverflow: IsOverflowNeeded());
            return _agents.Where(a => teams.Contains(a.Team)).ToList();
        }

        private int GetTeamCapacity(bool includeOverflow)
        {
            var teams = GetActiveTeams(includeOverflow);
            return _agents.Where(a => teams.Contains(a.Team)).Sum(a => a.MaxCapacity);
        }

        private bool IsOverflowNeeded()
        {
            int baseCapacity = GetTeamCapacity(false);
            int maxBaseQueue = (int)(baseCapacity * 1.5);
            return Queue.Count >= maxBaseQueue;
        }

        public (bool Success, Guid? SessionId, string Message) TryQueueChat()
        {
            int totalCapacity = GetTeamCapacity(IsOfficeHours);
            int maxQueueAllowed = (int)(totalCapacity * 1.5);

            if (Queue.Count >= maxQueueAllowed)
            {
                return (false, null, "Queue is full. Chat refused.");
            }

            var session = new ChatSession();
            Queue.Enqueue(session);
            ActiveSessions.TryAdd(session.Id, session);

            return (true, session.Id, "Chat queued successfully.");
        }

        public bool RecordPoll(Guid sessionId)
        {
            if (ActiveSessions.TryGetValue(sessionId, out var session))
            {
                session.LastPollTime = DateTime.UtcNow;
                return true;
            }
            return false;
        }

        public void AssignChats()
        {
            lock (_assignmentLock)
            {
                while (!Queue.IsEmpty)
                {
                    var activeAgents = GetActiveShiftAgents();

                    var availableAgents = activeAgents
                        .Where(a => a.CurrentChats < a.MaxCapacity)
                        .ToList();

                    if (availableAgents.Count == 0)
                        break;

                    var lowestTierAgents = availableAgents
                        .GroupBy(a => a.Seniority)
                        .OrderBy(g => g.Key)
                        .First()
                        .ToList();

                    var selectedAgent = lowestTierAgents
                        .OrderBy(a => a.LastAssigned)
                        .First();

                    if (Queue.TryDequeue(out var session))
                    {
                        if (session.IsActive)
                        {
                            selectedAgent.CurrentChats++;
                            selectedAgent.LastAssigned = DateTime.UtcNow;
                            session.AssignedAgentId = selectedAgent.Id;
                        }
                    }
                }
            }
        }

        public void CleanInactiveSessions()
        {
            var threshold = DateTime.UtcNow.AddSeconds(-3);

            foreach (var session in ActiveSessions.Values)
            {
                if (session.LastPollTime < threshold && session.IsActive)
                {
                    session.IsActive = false;

                    if (!string.IsNullOrEmpty(session.AssignedAgentId))
                    {
                        var agent = _agents.FirstOrDefault(a => a.Id == session.AssignedAgentId);
                        if (agent != null && agent.CurrentChats > 0)
                        {
                            agent.CurrentChats--;
                        }
                    }
                }
            }

            var inactiveIds = ActiveSessions.Values.Where(s => !s.IsActive).Select(s => s.Id).ToList();
            foreach (var id in inactiveIds) ActiveSessions.TryRemove(id, out _);
        }

        public object GetStatus() => new
        {
            QueueLength = Queue.Count,
            IsOfficeHours,
            TotalActiveSessions = ActiveSessions.Count,
            Agents = _agents.Select(a => new { a.Id, a.Team, a.Seniority, a.CurrentChats, a.MaxCapacity })
        };
    }
}
