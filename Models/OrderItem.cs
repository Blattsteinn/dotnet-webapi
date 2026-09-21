
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FoodOrderingApi.Models;

public class OrderItem
{
    public int Id { get; set; }

    public int OrderId { get; set; }
    public Order? Order { get; set; }

    public int ProductId { get; set; }
    public Product? Product { get; set; }

    public List<OrderItemAddOn> orderItemAddOns { get; set; } = [];

    // -- Snapshots from Product --------
    [Required]
    public required string Name { get; set; }

    public decimal Price { get; set; }
    // ----------------------------------

    public int Quantity { get; set; }
}