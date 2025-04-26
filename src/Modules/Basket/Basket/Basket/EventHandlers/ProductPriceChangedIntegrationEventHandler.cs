namespace Basket.Basket.EventHandlers;
public class ProductPriceChangedIntegrationEventHandler
    (ILogger<ProductPriceChangedIntegrationEventHandler> logger, ISender sender)
    : IConsumer<ProductPriceChangedIntegrationEvent>
{
    public Task Consume(ConsumeContext<ProductPriceChangedIntegrationEvent> context)
    {
        logger.LogInformation("Integration Event Handled : {integrationEvent}", context.Message.GetType().Name);

        // find basket items with product id and update price


        // mediatR new command and hanler to find products on basket and update price

        return Task.CompletedTask;

    }
}
