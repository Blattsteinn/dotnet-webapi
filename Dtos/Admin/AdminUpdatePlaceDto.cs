using System.ComponentModel.DataAnnotations;

namespace FoodOrderingApi.Dtos.Admin;

public record AdminUpdatePlaceDto(
    [Required]
    [MaxLength(200)]
    string Name,

    [Range(-90.0, 90.0)]
    double Latitude,

    [Range(-180.0, 180.0)]
    double Longtitude,

    [MaxLength(500)]
    string? Description,

    List<AdminUpdateCategoryDto> ProductCategories
);

public record AdminUpdateCategoryDto(
    // 0 = new node. Any other value must match an existing id under the parent.
    [Range(0, int.MaxValue)]
    int Id,

    [Required]
    [MaxLength(50)]
    string Name,

    [MaxLength(100)]
    string? Description,

    bool Hidden,

    List<AdminUpdateProductDto> Products
);


public record AdminUpdateProductDto(
    [Range(0, int.MaxValue)]
    int Id,

    [Required]
    [MaxLength(200)]
    string Name,

    [MaxLength(250)]
    string? Description,

    bool Hidden,

    [Range(typeof(decimal), "0.00", "10000")]
    decimal Price,

    List<AdminUpdateAddOnCategoryDto> AddOnCategories
);


public record AdminUpdateAddOnCategoryDto(
    [Range(0, int.MaxValue)]
    int Id,

    [Required]
    [MaxLength(200)]
    string Name,

    bool Required,

    bool Multichoice,

    List<AdminUpdateAddOnDto> AddOns
);


public record AdminUpdateAddOnDto(
    [Range(0, int.MaxValue)]
    int Id,

    [Required]
    [MaxLength(200)]
    string Name,

    [Range(typeof(decimal), "0.00", "10000")]
    decimal Price,

    bool Hidden
);