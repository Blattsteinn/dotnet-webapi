
namespace FoodOrderingApi.Models;
public class AddOnCategory
{
    public int Id { get; set; }
    public int ProductId { get; set; }
    public Product? Product { get; set; }
    public List<AddOn> AddOns { get; set; } = [];
    public required string Name { get; set; }
    public bool Required { get; set; } = false;

    public bool Multichoice { get; set; } = false;

   
}