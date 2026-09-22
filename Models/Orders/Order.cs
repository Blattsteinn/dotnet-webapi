
using FoodOrderingApi.Models.Places;
using System.ComponentModel.DataAnnotations;

namespace FoodOrderingApi.Models.Orders;

public class Order
{
    public int Id { get; set; }

    // Note: FK can be null so that we could delete place 
    public int? PlaceId { get; set; }

    public Place? Place { get; set; }
    public List<OrderItem> OrderItems { get; set; } = [];

    // User table does not exist yet
    // public int UserId { get; set; }

    [EnumDataType(typeof(OrderStatus))]
    public OrderStatus Status { get; set; } = OrderStatus.Pending;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

public enum OrderStatus
{
    Pending,
    Confirmed,
    Preparing,
    Ready,
    Completed,
    Cancelled
}