
using FoodOrderingApi.Models.Orders;

namespace FoodOrderingApi.Dtos.Orders;
public record GetOrderDto(
    int Id,
    int? PlaceId,
    OrderStatus Status,
    DateTime CreatedAt,
    List<GetOrderItemDto> OrderItems
);

public record GetOrderItemDto(
    int Id,
    int? ProductId,
    string Name,
    decimal Price,
    int Quantity,
    List<GetOrderItemAddOnDto> OrderItemAddOns
);

public record  GetOrderItemAddOnDto(
    int Id,
    int? AddOnId,
    string Name,
    decimal Price,
    int Quantity
);