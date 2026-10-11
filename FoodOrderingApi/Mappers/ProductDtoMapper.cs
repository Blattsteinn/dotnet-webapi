
using FoodOrderingApi.Dtos.Places;
using FoodOrderingApi.Models.Places;

namespace FoodOrderingApi.Mappers;
public static class ProductDtoMapper
{
    // To use this, tables must be joined:
        // Product? product = _db.Products
        //     .Include(p => p.AddOnCategories)
        //     .ThenInclude(aoc => aoc.AddOns)
        //     .FirstOrDefault(p => p.Id == id && p.ProductCategoryId == productCategoryId);
    public static GetProductDto MapToGetDto(Product product)
        {
            return new GetProductDto(
                product.Id,
                product.Name,
                product.Description,
                product.Price,
                product.AddOnCategories.Select(aoc => new AddOnCategoriesDetailDto(
                    aoc.Id,
                    aoc.Name,
                    aoc.Required,
                    aoc.Multichoice,
                    aoc.AddOns.Select(ao => new AddOnDetailDto(
                        ao.Id,
                        ao.Name,
                        ao.Price
                    )).ToList()
                )).ToList()
            );
        }

    // Flat list entry. No Hidden, because the caller only ever sees the
    // products that survived the visibility filter.
    public static ProductSummaryDto MapToSummaryDto(Product product)
        {
            return new ProductSummaryDto(
                product.Id,
                product.Name,
                product.Description,
                product.Price
            );
        }
}
    