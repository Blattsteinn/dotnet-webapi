using System.ComponentModel.DataAnnotations;

namespace FoodOrderingApi.Models.Places;

public class ProductCategory
{
    public int Id { get; set; }

    [Required]
    [MaxLength(50)]
    public required string Name { get; set; }

    [MaxLength(100)]
    public string? Description { get; set; }

    public bool Hidden { get; set; } = true;

    public int PlaceId { get; set; }
    public Place? Place { get; set; }


    public List<Product> Products { get; set; } = [];
}