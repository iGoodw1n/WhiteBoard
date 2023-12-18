
using ApiBoard.Services;

namespace ApiBoard.Workers
{
    public class CleanCacheService : BackgroundService
    {
        readonly Action _cleanCache;

        public CleanCacheService(BoardCloudStorageService storageService)
        {
            (_, _cleanCache) = storageService.GetHandlers();
        }
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            using var timer = new PeriodicTimer(TimeSpan.FromMinutes(10));

            while (!stoppingToken.IsCancellationRequested &&
                await timer.WaitForNextTickAsync(stoppingToken))
            {
                _cleanCache();
            }
        }
    }
}
