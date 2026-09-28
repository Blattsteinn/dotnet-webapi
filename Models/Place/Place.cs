using System.ComponentModel.DataAnnotations;

namespace FoodOrderingApi.Models.Places;

public class Place
{
    public int Id { get; set;}

    [Range(-90.0, 90.0)]
    public double Latitude { get; set; }

    [Range(-90.0, 90.0)]
    public double Longtitude { get; set; }

    [Required]
    [MaxLength(200)]
    public required string Name { get; set; }

    [MaxLength(500)]
    public string? Description { get; set; }

    public List<ProductCategory> ProductCategories { get; set; } = [];
}
