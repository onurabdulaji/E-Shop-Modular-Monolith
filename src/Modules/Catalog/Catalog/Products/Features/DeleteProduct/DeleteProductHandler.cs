namespace Catalog.Products.Features.DeleteProduct;

public record DeleteProductCommand(Guid productId) : ICommand<DeleteProductResult>;

public record DeleteProductResult(bool IsSuccess);

public class DeleteProductCommandValidator : AbstractValidator<DeleteProductCommand>
{
    public DeleteProductCommandValidator()
    {
        RuleFor(x => x.productId).NotEmpty().WithMessage("Id is required.");
    }
}

internal class DeleteProductHandler(CatalogDbContext dbContext)
    : ICommandHandler<DeleteProductCommand, DeleteProductResult>
{
    public async Task<DeleteProductResult> Handle(DeleteProductCommand command, CancellationToken cancellationToken)
    {
        // Delete prodcut from command object
        // save to db
        // return result

        var product = await dbContext.Products
            .FindAsync([command.productId], cancellationToken);

        if (product is null) throw new ProductNotFoundException(command.productId);

        dbContext.Products.Remove(product);

        await dbContext.SaveChangesAsync(cancellationToken);

        return new DeleteProductResult(true);

    }
}
