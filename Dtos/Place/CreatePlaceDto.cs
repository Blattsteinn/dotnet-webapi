using System.ComponentModel.DataAnnotations;

namespace FoodOrderingApi.Dtos.Places;

// Body for POST /api/v1/owner/places.
// There is deliberately no owner field: the owner is always the caller, so a
// client can never claim a place on someone else's behalf.
public record CreatePlaceDto(
    [Required]
    [MaxLength(200)]
    string Name,

    [Range(-90.0, 90.0)]
    double Latitude,

    [Range(-180.0, 180.0)]
    double Longtitude,

    [MaxLength(500)]
    string? Description
);
