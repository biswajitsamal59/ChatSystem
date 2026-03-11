using ChatAPI.Services;

namespace ChatAPI.Workers
{
    public class MonitorWorker(ChatManager chatManager) : BackgroundService
    {
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            using var timer = new PeriodicTimer(TimeSpan.FromSeconds(1));
            while (await timer.WaitForNextTickAsync(stoppingToken))
            {
                chatManager.CleanInactiveSessions();
            }
        }
    }
}
