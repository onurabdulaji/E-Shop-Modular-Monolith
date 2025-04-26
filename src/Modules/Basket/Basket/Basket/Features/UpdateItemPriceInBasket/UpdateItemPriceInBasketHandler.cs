namespace Basket.Basket.Features.UpdateItemPriceInBasket;

public record UpdateItemPriceInBasketCommand(Guid ProductId, decimal Price) : ICommand<UpdateItemPriceInBasketResult>;

public record UpdateItemPriceInBasketResult(bool IsSuccess);

public class UpdateItemPriceInBasketCommandValidator : AbstractValidator<UpdateItemPriceInBasketCommand>
{
    public UpdateItemPriceInBasketCommandValidator()
    {
        RuleFor(q => q.ProductId).NotEmpty().WithMessage("ProductId is required");
        RuleFor(q => q.Price).GreaterThan(0).WithMessage("Price must be greater than zero.");
    }
}

internal class UpdateItemPriceInBasketHandler
    (BasketDbContext dbContext)
    : ICommandHandler<UpdateItemPriceInBasketCommand, UpdateItemPriceInBasketResult>
{
    public async Task<UpdateItemPriceInBasketResult> Handle(UpdateItemPriceInBasketCommand command, CancellationToken cancellationToken)
    {
        // find shopping cart items with a productId 
        // Iterate items and Update price every time
        // save to db 
        // return result

        var itemsToUpdate = await dbContext.ShoppingCartItems
            .Where(q => q.ProductId == command.ProductId)
            .ToListAsync(cancellationToken);

        if (!itemsToUpdate.Any()) return new UpdateItemPriceInBasketResult(false);

        foreach (var item in itemsToUpdate)
        {
            item.UpdatePrice(command.Price);
        }

        await dbContext.SaveChangesAsync(cancellationToken);

        return new UpdateItemPriceInBasketResult(true);
    }
}
