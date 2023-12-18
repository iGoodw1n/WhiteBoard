using ApiBoard.Services;

namespace ApiBoard.Workers;

public class TimedHostedService : BackgroundService, IDisposable
{
    readonly Action _saveChanges;
    readonly Action _cleanCache;
    private Timer? _timerForSaving = null;
    private Timer? _timerForCleaning = null;

    public TimedHostedService(BoardCloudStorageService storageService)
    {
        (_saveChanges, _cleanCache) = storageService.GetHandlers();
    }

    public Task StartAsync(CancellationToken stoppingToken)
    {

        _timerForSaving = new Timer((_) => _saveChanges(), null, TimeSpan.FromSeconds(1),
            Timeout.InfiniteTimeSpan);
        _timerForCleaning = new Timer((_) => _cleanCache(), null, TimeSpan.FromMinutes(10),
            Timeout.InfiniteTimeSpan);

        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken stoppingToken)
    {
        _timerForSaving?.Change(Timeout.Infinite, 0);
        _timerForCleaning?.Change(Timeout.Infinite, 0);

        return Task.CompletedTask;
    }

    public void Dispose()
    {
        _timerForSaving?.Dispose();
        _timerForCleaning?.Dispose();
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
