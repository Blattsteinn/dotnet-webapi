
using FoodOrderingApi.Dtos.Places;
using FoodOrderingApi.Models.Places;

namespace FoodOrderingApi.Mappers;
public static class PlaceDtoMapper
{
    // To use this, tables must be joined:
            // Order? order = _db.Orders
            // .Include(o => o.OrderItems)
            // .ThenInclude(oi => oi.OrderItemAddOns)
            // .FirstOrDefault(o => o.Id  == id);
    public static PlaceSummaryDto MapToSummaryDto(Place place)
        {
            return new PlaceSummaryDto(
                Id: place.Id,
                Name: place.Name,
                Description: place.Description,
                Address: place.Address,
                Latitude: place.Latitude,
                Longtitude: place.Longtitude,
                Hidden: place.Hidden
            );
        }

    public static GetPlaceDto MapToGetDto(Place place)
        {
            return new GetPlaceDto(
                Id: place.Id,
                Latitude: place.Latitude,
                Longtitude: place.Longtitude,
                Name: place.Name,
                Description: place.Description,
                Address: place.Address,
                Hidden: place.Hidden,
                ProductCategories: place.ProductCategories
                    .Select(category => new ProductCategoriesDetailDto(
                        Id: category.Id,
                        Name: category.Name,
                        Hidden: category.Hidden,
                        Products: category.Products
                            .Select(product => new ProductSummaryDto(
                                Id: product.Id,
                                Name: product.Name,
                                Hidden: product.Hidden,
                                Description: product.Description,
                                Price: product.Price
                            ))
                            .ToList()
                    ))
                    .ToList()
            );
        }
}
    