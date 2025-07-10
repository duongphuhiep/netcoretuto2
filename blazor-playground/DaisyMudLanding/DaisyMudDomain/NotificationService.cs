using Microsoft.Extensions.Logging;

namespace DaisyMudDomain;

public class NotificationService(ILogger<NotificationService> _logger)
{
    public async ValueTask<Notification[]> GetNotifications(CancellationToken cancellationToken)
    {
        _logger.LogInformation("GetNotifications");

        //Simulate getting data from external sources
        await Task.Delay(SimulationOptions.DelaySeconds * 1000, cancellationToken);

        return
        [
            new Notification
            {
                Title = "v8: Riding the Momentum of v7",
                Author = "The MudBlazor Team",
                Date = "Jan 19, 2025"
            },
            new Notification
            {
                Title = "v7 Is Here",
                Author = "The MudBlazor Team",
                Date = "Jun 29, 2024"
            },
            new Notification
            {
                Title = "MudBlazor is here to stay",
                Author = "Jonny Larsson",
                Date = "Jan 13, 2022"
            }
        ];
    }
}