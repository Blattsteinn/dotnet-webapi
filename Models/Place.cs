using System.ComponentModel.DataAnnotations;

namespace FoodOrderingApi.Models;

public class Place
{
    public int Id { get; set;}

    public double Latitude { get; set; }
    public double Longtitude { get; set; }

    [Required]
    public required string Name { get; set; }

    public string? Description { get; set; }

    public List<ProductCategory> ProductCategories { get; set; } = [];
}
