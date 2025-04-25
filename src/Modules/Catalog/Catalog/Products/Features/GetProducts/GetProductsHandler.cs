using Shared.Pagination;

namespace Catalog.Products.Features.GetProducts;

public record GetProductsQuery(PaginationRequest PaginationRequest)
    : IQuery<GetProductsResult>;

public record GetProductsResult(PaginatedResult<ProductDto> Products);

internal class GetProductsHandler(CatalogDbContext dbContext)
    : IQueryHandler<GetProductsQuery, GetProductsResult>
{
    public async Task<GetProductsResult> Handle(GetProductsQuery query, CancellationToken cancellationToken)
    {
        // get products using context
        // map with Mapster
        // return result

        var pageIndex = query.PaginationRequest.pageIndex;
        var pagaSize = query.PaginationRequest.pageSize;

        var totalCount = await dbContext.Products.LongCountAsync(cancellationToken);

        var products = await dbContext.Products
            .AsNoTracking()
            .OrderBy(p => p.Name)
            .Skip(pageIndex * pagaSize)
            .Take(pagaSize)
            .ToListAsync(cancellationToken);

        var productsDtos = products.Adapt<List<ProductDto>>();

        return new GetProductsResult
            (new PaginatedResult<ProductDto>(
            pageIndex, pagaSize, totalCount, productsDtos)
            );
    }
}
