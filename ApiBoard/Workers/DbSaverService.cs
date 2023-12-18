using ApiBoard.Services;

namespace ApiBoard.Workers;

public class DbSaverService : BackgroundService
{
    readonly Action _saveChanges;

    public DbSaverService(BoardCloudStorageService storageService)
    {
        (_saveChanges, _) = storageService.GetHandlers();
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(TimeSpan.FromSeconds(1));

        while (!stoppingToken.IsCancellationRequested &&
            await timer.WaitForNextTickAsync(stoppingToken))
        {
            _saveChanges();
        }
    }
}

    
