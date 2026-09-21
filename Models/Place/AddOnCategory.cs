using System.ComponentModel.DataAnnotations;

namespace FoodOrderingApi.Models.Places;
public class AddOnCategory
{
    public int Id { get; set; }
    public int ProductId { get; set; }
    public Product? Product { get; set; }
    public List<AddOn> AddOns { get; set; } = [];

    [Required]
    public required string Name { get; set; }
    public bool Required { get; set; } = false;

    public bool Multichoice { get; set; } = false;

   
}