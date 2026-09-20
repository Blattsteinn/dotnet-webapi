using System.ComponentModel.DataAnnotations;

namespace FoodOrderingApi.Models;

public class ProductCategory
{
    public int Id { get; set; }

    [Required]
    public required string Name { get; set; }
    public string? Description { get; set; }

    public bool Hidden { get; set; } = true;

    public int PlaceId { get; set; }
    public Place? Place { get; set; }


    public List<Product> Products { get; set; } = [];
}