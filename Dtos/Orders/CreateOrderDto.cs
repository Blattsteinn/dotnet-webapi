using System.ComponentModel.DataAnnotations;

namespace FoodOrderingApi.Dtos.Orders;
public record CreateOrderDto(
    int PlaceId,
    List<CreateOrderItemDto> OrderItems
);

public record CreateOrderItemDto(
    int ProductId,
    [Range(1, int.MaxValue)] 
    int Quantity,
    List<CreateOrderItemAddOnDto> AddOns
);

public record CreateOrderItemAddOnDto(
    int AddOnId,
    [Range(1, int.MaxValue)] 
    int Quantity
);
