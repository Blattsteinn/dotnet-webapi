using System.ComponentModel.DataAnnotations;

namespace FoodOrderingApi.Dtos.Accounts.Owner;

// Body for PATCH /api/v1/owner/places/{placeId}/managers/roles.
// The owner assigns a manager by the person's email, not their internal id.
public record AssignManagerDto(
    [Required]
    [EmailAddress]
    [MaxLength(256)]
    string Email
);
