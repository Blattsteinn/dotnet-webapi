using System.ComponentModel.DataAnnotations;

namespace FoodOrderingApi.Dtos.Orders;

public record CreateOrderItemAddOnDto(
    int OrderItemId,
    int AddOnId,
    [Range(1, int.MaxValue)] 
    int Quantity
);
