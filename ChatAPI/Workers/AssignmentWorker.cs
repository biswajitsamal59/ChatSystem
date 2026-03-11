using ChatAPI.Services;

namespace ChatAPI.Workers
{
    public class AssignmentWorker(ChatManager chatManager) : BackgroundService
    {
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            using var timer = new PeriodicTimer(TimeSpan.FromMilliseconds(500));
            while (await timer.WaitForNextTickAsync(stoppingToken))
            {
                chatManager.AssignChats();
            }
        }
    }
}
