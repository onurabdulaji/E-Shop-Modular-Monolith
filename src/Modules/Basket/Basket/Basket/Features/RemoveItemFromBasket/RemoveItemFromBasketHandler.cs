namespace Basket.Basket.Features.RemoveItemFromBasket;

public record RemoveItemFromBasketCommand(string UserName, Guid ProductId)
    : ICommand<RemoveItemFromBasketResult>;

public record RemoveItemFromBasketResult(Guid Id);

public class RemoveItemFromBasketCommandValitor : AbstractValidator<RemoveItemFromBasketCommand>
{
    public RemoveItemFromBasketCommandValitor()
    {
        RuleFor(x => x.UserName).NotEmpty().WithMessage("User name is required.");

        RuleFor(x => x.ProductId).NotEmpty().WithMessage("Product ID is required.");
    }
}

internal class RemoveItemFromBasketHandler
    (IBasketRepository repository)
    : ICommandHandler<RemoveItemFromBasketCommand, RemoveItemFromBasketResult>
{
    public async Task<RemoveItemFromBasketResult> Handle(RemoveItemFromBasketCommand command, CancellationToken cancellationToken)
    {
        var shoppingCart = await repository.GetBasket(command.UserName, false, cancellationToken);

        shoppingCart.RemoveItem(command.ProductId);

        await repository.SaveChangesAsync(command.UserName, cancellationToken);

        return new RemoveItemFromBasketResult(shoppingCart.Id);
    }
}
