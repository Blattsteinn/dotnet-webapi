using FoodOrderingApi.Models.Users;

namespace FoodOrderingApi.Models.Places;

// One row per place: "this user is allowed to edit this place".
//
// OwnerId deliberately has no unique constraint — that is what lets a single
// owner hold several places. PlaceId IS unique, which caps a place at exactly
// one owner. Together they model one-owner-to-many-places.
//
// The row is written by the system when an owner creates a place, never by the
// client: ownership is a consequence of creating a place, not a field you set.
public class PlaceOwnership
{
    public int Id { get; set; }

    public int PlaceId { get; set; }
    public Place Place { get; set; } = null!;

    public required string OwnerId { get; set; }
    public ApplicationUser Owner { get; set; } = null!;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
