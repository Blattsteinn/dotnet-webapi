using System.ComponentModel.DataAnnotations;

using FoodOrderingApi.Models.Places;

namespace FoodOrderingApi.Models.Orders;

public class OrderItem
{
    public int Id { get; set; }

    public int OrderId { get; set; }
    public Order? Order { get; set; }

    public int? ProductId { get; set; }
    public Product? Product { get; set; }

    public List<OrderItemAddOn> OrderItemAddOns { get; set; } = [];

    // -- snapshot values from product --------
    [Required]
    public required string Name { get; set; }

    [Range(0, 10000)]
    public decimal Price { get; set; }
    // ----------------------------------------
    
    [Range(1, 1000)]
    public int Quantity { get; set; }
}