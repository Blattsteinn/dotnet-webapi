
namespace FoodOrderingApi.Models;
public class Order
{
    public int Id { get; set; }
    public int PlaceId { get; set; }

    public Place? Place { get; set; }
    public List<OrderItem> OrderItems { get; set; } = [];

    // User table does not exist yet
    // public int UserId { get; set; }

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