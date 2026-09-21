using System.ComponentModel.DataAnnotations;

namespace FoodOrderingApi.Dtos.OrderItem;

public record CreateOrderItemDto(
    int ProductId,
    [Range(1, int.MaxValue)] 
    int Quantity
);
