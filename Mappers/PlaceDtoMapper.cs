
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
    public static PlaceSummaryDto MapToSummaryDto(Place place, bool isOpen)
        {
            return new PlaceSummaryDto(
                Id: place.Id,
                Name: place.Name,
                Description: place.Description,
                Address: place.Address,
                Latitude: place.Latitude,
                Longtitude: place.Longtitude,
                AcceptingOrders: place.AcceptingOrders,
                IsOpen: isOpen,
                OpeningHours: MapHours(place)
            );
        }

    public static PrivatePlaceDto MapToPrivateDto(Place place, bool isOpen)
        {
            return new PrivatePlaceDto(
                Id: place.Id,
                Latitude: place.Latitude,
                Longtitude: place.Longtitude,
                Name: place.Name,
                Description: place.Description,
                Address: place.Address,
                Hidden: place.Hidden,
                AcceptingOrders: place.AcceptingOrders,
                IsOpen: isOpen,
                OpeningHours: MapHours(place),
                ProductCategories: MapCategories(place)
            );
        }

    public static PublicPlaceDto MapToPublicDto(Place place, bool isOpen)
        {
            return new PublicPlaceDto(
                Id: place.Id,
                Latitude: place.Latitude,
                Longtitude: place.Longtitude,
                Name: place.Name,
                Description: place.Description,
                Address: place.Address,
                AcceptingOrders: place.AcceptingOrders,
                IsOpen: isOpen,
                OpeningHours: MapHours(place),
                ProductCategories: MapCategories(place)
            );
        }

    // Ordered so the schedule reads like a week: day first, then opening time.
    public static List<OpeningHourDto> MapHours(Place place) =>
        place.OpeningHours
            .OrderBy(hour => hour.Day)
            .ThenBy(hour => hour.OpensAt)
            .Select(hour => new OpeningHourDto(hour.Day, hour.OpensAt, hour.ClosesAt))
            .ToList();

    private static List<ProductCategoriesDetailDto> MapCategories(Place place) =>
        place.ProductCategories
            .Select(category => new ProductCategoriesDetailDto(
                Id: category.Id,
                Name: category.Name,
                Hidden: category.Hidden,
                Products: category.Products
                    .Select(product => new PlaceProductDto(
                        Id: product.Id,
                        Name: product.Name,
                        Hidden: product.Hidden,
                        Description: product.Description,
                        Price: product.Price
                    ))
                    .ToList()
            ))
            .ToList();
}
    