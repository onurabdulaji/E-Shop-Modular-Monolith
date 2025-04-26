namespace Catalog.Products.EventHandlers;
public class ProductPriceChangedEventHandler
    (ILogger<ProductPriceChangedEventHandler> logger, IBus bus)
    : INotificationHandler<ProductPriceChangedEvent>
{
    public async Task Handle(ProductPriceChangedEvent notification, CancellationToken cancellationToken)
    {
        logger.LogInformation("Domain Event handled: {DomainEvent}", notification.GetType().Name);

        var intgrationEvent = new ProductPriceChangedIntegrationEvent
        {
            ProductId = notification.Product.Id,
            Name = notification.Product.Name,
            Category = notification.Product.Category,
            Description = notification.Product.Description,
            ImageFile = notification.Product.ImageFile,
            Price = notification.Product.Price, // set updated prodcut price
        };

        await bus.Publish(intgrationEvent, cancellationToken);

    }
}
