public class OrderItemAddOn
{
    public int Id { get; set; }
    public int OrderItemId { get; set; }
    public int AddOnId { get; set; }
    
    // snapshot values ---------------
    public required string Name { get; set; }
    public decimal Price { get; set; }
    //------------------------------

    // per single unit of the parent 
    // OrderItem — total = OrderItem.quantity * this.quantity
    public int Quantity { get; set; }

}