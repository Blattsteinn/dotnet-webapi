
namespace FoodOrderingApi.Models;
public class Order
{
    public int Id { get; set; }
    public int PlaceId { get; set; }

    public Place? Place { get; set; }

    // User table does not exist yet
    // public int UserId { get; set; }

    public OrderStatus Status { get; set; }

    public DateTime CreatedAt { get; set; }
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