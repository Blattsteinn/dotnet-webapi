using System.ComponentModel.DataAnnotations;

namespace FoodOrderingApi.Models;
public class OrderItemAddOn
{
    public int Id { get; set; }
    
    public int OrderItemId { get; set; }
    public OrderItem? OrderItem { get; set; }

    public int AddOnId { get; set; }
    public AddOn? AddOn { get; set; }
    
    // snapshot values from AddOn ---------------
    [Required]
    public required string Name { get; set; }

    [Range(0, 10000)]
    public decimal Price { get; set; }
    //------------------------------------------

    // per single unit of the parent 
    // OrderItem — total = OrderItem.quantity * this.quantity

    [Range(1, 1000)]
    public int Quantity { get; set; }

}