using Xunit;
using ChatAPI.Services;
using ChatAPI.Models;

namespace ChatAPI.Tests
{
    public class ChatManagerTests
    {
        [Fact]
        public void TryQueueChat_ShouldAddSession_WhenQueueIsNotFull()
        {
            // Arrange
            var manager = new ChatManager();

            // Act
            var (success, sessionId, message) = manager.TryQueueChat();

            // Assert
            Assert.True(success);
            Assert.NotNull(sessionId);
            Assert.Equal("Chat queued successfully.", message);
            Assert.Single(manager.Queue);
            Assert.Single(manager.ActiveSessions);
        }

        [Fact]
        public void RecordPoll_ShouldUpdateLastPollTime_WhenSessionExists()
        {
            // Arrange
            var manager = new ChatManager();
            var (_, sessionId, _) = manager.TryQueueChat();
            var session = manager.ActiveSessions[sessionId!.Value];

            // Artificially "age" the session so we can ensure the poll updates it
            var oldTime = DateTime.UtcNow.AddSeconds(-5);
            session.LastPollTime = oldTime;

            // Act
            var pollResult = manager.RecordPoll(sessionId.Value);

            // Assert
            Assert.True(pollResult);
            Assert.True(session.LastPollTime > oldTime, "LastPollTime should be updated to a newer UTC time.");
        }

        [Fact]
        public void RecordPoll_ShouldReturnFalse_WhenSessionDoesNotExist()
        {
            // Arrange
            var manager = new ChatManager();
            var fakeId = Guid.NewGuid();

            // Act
            var pollResult = manager.RecordPoll(fakeId);

            // Assert
            Assert.False(pollResult);
        }

        [Fact]
        public void AssignChats_ShouldAssignAgent_AndKeepSessionActive()
        {
            // Arrange
            var manager = new ChatManager();
            manager.TryQueueChat();

            // Act
            manager.AssignChats();

            // Assert
            // The queue should be empty because it was dequeued and assigned
            Assert.Empty(manager.Queue);

            var activeSession = manager.ActiveSessions.Values.First();
            Assert.NotNull(activeSession.AssignedAgentId);
            Assert.True(activeSession.IsActive);
        }

        [Fact]
        public void CleanInactiveSessions_ShouldRemoveSession_WhenMissedThreePolls()
        {
            // Arrange
            var manager = new ChatManager();
            manager.TryQueueChat();
            var session = manager.ActiveSessions.Values.First();

            // Simulate a timeout (older than 3 seconds)
            session.LastPollTime = DateTime.UtcNow.AddSeconds(-4);

            // Act
            manager.CleanInactiveSessions();

            // Assert
            Assert.Empty(manager.ActiveSessions); // It should be completely removed from the dictionary
        }

        [Fact]
        public void CleanInactiveSessions_ShouldKeepSession_WhenPollIsRecent()
        {
            // Arrange
            var manager = new ChatManager();
            manager.TryQueueChat();
            var session = manager.ActiveSessions.Values.First();

            // Simulate a recent poll (only 1 second ago)
            session.LastPollTime = DateTime.UtcNow.AddSeconds(-1);

            // Act
            manager.CleanInactiveSessions();

            // Assert
            Assert.Single(manager.ActiveSessions); // It should survive the cleanup
            Assert.True(session.IsActive);
        }
    }
}
