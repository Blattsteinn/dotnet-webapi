using System.ComponentModel.DataAnnotations;

namespace FoodOrderingApi.Dtos.Orders;

public record CreateOrderItemDto(
    int ProductId,
    [Range(1, int.MaxValue)] 
    int Quantity
);
