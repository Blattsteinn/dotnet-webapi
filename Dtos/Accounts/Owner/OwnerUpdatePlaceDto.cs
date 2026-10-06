using System.ComponentModel.DataAnnotations;

namespace FoodOrderingApi.Dtos.Accounts.Owner;

public record OwnerUpdatePlaceDto(
    [Required]
    [MaxLength(200)]
    string Name,

    [Range(-90.0, 90.0)]
    double Latitude,

    [Range(-180.0, 180.0)]
    double Longtitude,

    [MaxLength(500)]
    string? Description,

    bool Hidden,
    
    List<OwnerUpdateCategoryDto> ProductCategories
);

public record OwnerUpdateCategoryDto(
    // 0 = new node. Any other value must match an existing id under the parent.
    [Range(0, int.MaxValue)]
    int Id,

    bool Destroy,

    [Required]
    [MaxLength(50)]
    string Name,

    [MaxLength(100)]
    string? Description,

    bool Hidden,

    List<OwnerUpdateProductDto> Products
);


public record OwnerUpdateProductDto(
    [Range(0, int.MaxValue)]
    int Id,

    bool Destroy,

    [Required]
    [MaxLength(200)]
    string Name,

    [MaxLength(250)]
    string? Description,

    bool Hidden,

    [Range(typeof(decimal), "0.00", "10000")]
    decimal Price,

    List<OwnerUpdateAddOnCategoryDto> AddOnCategories
);


public record OwnerUpdateAddOnCategoryDto(
    [Range(0, int.MaxValue)]
    int Id,

    bool Destroy,

    [Required]
    [MaxLength(200)]
    string Name,

    bool Required,

    bool Multichoice,

    List<OwnerUpdateAddOnDto> AddOns
);


public record OwnerUpdateAddOnDto(
    [Range(0, int.MaxValue)]
    int Id,

    bool Destroy,

    [Required]
    [MaxLength(200)]
    string Name,

    [Range(typeof(decimal), "0.00", "10000")]
    decimal Price,

    bool Hidden
);