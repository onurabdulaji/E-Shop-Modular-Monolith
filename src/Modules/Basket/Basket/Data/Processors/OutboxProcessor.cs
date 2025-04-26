using Microsoft.Extensions.DependencyInjection;

namespace Basket.Data.Processors;
public class OutboxProcessor
    (IServiceProvider serviceProvider, IBus bus, ILogger<OutboxProcessor> logger)
    : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = serviceProvider.CreateScope();

                var dbContext = scope.ServiceProvider.GetRequiredService<BasketDbContext>();

                var outBoxMessages = await dbContext.OutboxMessages
                    .Where(q => q.ProcessedOn == null)
                    .ToListAsync(stoppingToken);

                foreach (var message in outBoxMessages)
                {
                    var eventType = Type.GetType(message.Type);

                    if (eventType == null) continue; // logger can be added

                    var eventMessage = JsonSerializer.Deserialize(message.Content, eventType);

                    if (eventType == null) continue; // logger can be added

                    await bus.Publish(eventMessage, stoppingToken);

                    message.ProcessedOn = DateTime.UtcNow;

                    // logger ProcessedOn can be added
                }

                await dbContext.SaveChangesAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error processing outbox message");
            }

            await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken);
        }
    }
}
