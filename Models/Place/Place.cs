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

    [MaxLength(500)]
    public string? Address { get; set; }

    // Set by the system when the owning user loses the Owner role, or by an
    // admin directly. Hidden places disappear from the public endpoints.
    public bool Hidden { get; set; }

    // The ownership row that links this place to its owner. Null for places
    // that were never created by an owner (seed data, admin-created places).
    public PlaceOwnership? Ownership { get; set; }

    public List<ProductCategory> ProductCategories { get; set; } = [];
}
