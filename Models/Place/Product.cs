using System.ComponentModel.DataAnnotations;

namespace FoodOrderingApi.Models.Places;
public class Product
{
    public int Id { get; set;}

    public int ProductCategoryId { get; set; }

    public ProductCategory? ProductCategory { get; set; }

    public List<AddOnCategory> AddOnCategories { get; set; } = [];

    [Required]
    public required string Name { get; set; }

    [MaxLength(120)]
    public string? Description { get; set; }

    public bool Hidden { get; set; } = true;

    [Required]
    [Range(typeof(decimal), "0.00", "10000")]
    public decimal Price { get; set; }
}