using FoodOrderingApi.Models.Users;

namespace FoodOrderingApi.Models.Places;

// One row per (place, user): "this user manages this place".
//
// Unlike PlaceOwnership, this is many-to-many — a place may have several
// managers and the same user may manage several places — so the identity of
// the row is the composite key (PlaceId, UserId).
//
// The row is written by the owner of the place; it is not an Identity role, so
// it grants nothing globally.
public class PlaceManager
{
    public int PlaceId { get; set; }
    public Place Place { get; set; } = null!;

    public required string UserId { get; set; }
    public ApplicationUser User { get; set; } = null!;
}
