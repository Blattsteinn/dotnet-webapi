using System.ComponentModel.DataAnnotations;

namespace FoodOrderingApi.Models;
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
    [Range(0.00, double.MaxValue)]
    public decimal Price { get; set; }
}