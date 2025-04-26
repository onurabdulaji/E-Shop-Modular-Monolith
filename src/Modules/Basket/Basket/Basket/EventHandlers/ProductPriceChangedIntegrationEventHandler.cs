using Basket.Basket.Features.UpdateItemPriceInBasket;

namespace Basket.Basket.EventHandlers;
public class ProductPriceChangedIntegrationEventHandler
    (ILogger<ProductPriceChangedIntegrationEventHandler> logger, ISender sender)
    : IConsumer<ProductPriceChangedIntegrationEvent>
{
    public async Task Consume(ConsumeContext<ProductPriceChangedIntegrationEvent> context)
    {
        logger.LogInformation("Integration Event Handled : {integrationEvent}", context.Message.GetType().Name);

        var command = new UpdateItemPriceInBasketCommand(context.Message.ProductId, context.Message.Price);

        var result = await sender.Send(command);

        if (!result.IsSuccess)
            logger.LogError("Error updating price : {ProductId}", context.Message.ProductId);

        logger.LogInformation("Price for product id : {ProductId} updated in Basket", context.Message.ProductId);
    }
}
