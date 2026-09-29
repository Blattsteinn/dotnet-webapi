
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
                product.Hidden,
                product.AddOnCategories.Select(aoc => new AddOnCategoriesDetailDto(
                    aoc.Id,
                    aoc.Name,
                    aoc.Required,
                    aoc.Multichoice,
                    aoc.AddOns.Select(ao => new AddOnDetailDto(
                        ao.Id,
                        ao.Name,
                        ao.Price,
                        ao.Hidden
                    )).ToList()
                )).ToList()
            );
        }
}
    