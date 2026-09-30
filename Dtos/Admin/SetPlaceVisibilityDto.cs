using System.ComponentModel.DataAnnotations;

namespace FoodOrderingApi.Dtos.Admin;

// Body for PATCH /api/v1/admin/places/{id}/visibility
public record SetPlaceVisibilityDto(
    bool Hidden
);
