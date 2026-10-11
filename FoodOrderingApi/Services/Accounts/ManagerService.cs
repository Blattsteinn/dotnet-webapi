using FoodOrderingApi.Data;
using FoodOrderingApi.Dtos.Accounts.Owner;
using FoodOrderingApi.Mappers;
using FoodOrderingApi.Models.Places;
using FoodOrderingApi.Services.Places;
using Microsoft.EntityFrameworkCore;


namespace FoodOrderingApi.Services.Accounts;


// Read-only view of the places a caller manages. The caller id always comes
// from the token — never from the request.
//
// Scoping mirrors OrderService.ManagesPlace: an ownership row only counts while
// the caller still holds the Owner role, and an assigned Managers row grants
// control on its own. So an owner is a manager of their own place, which is why
// the /manager routes serve owners too.
public class ManagerService
{
    private readonly AppDbContext _db;
    private readonly PlaceScheduleService _schedule;

    public ManagerService(AppDbContext db, PlaceScheduleService schedule)
    {
        _db = db;
        _schedule = schedule;
    }

    public List<OwnedPlaceDto> GetManagedPlaces(string userId, bool isOwner)
    {
        return _db.Places
            .Include(p => p.OpeningHours)
            .Where(p =>
                (isOwner && p.Ownership != null && p.Ownership.OwnerId == userId) ||
                _db.Managers.Any(m => m.PlaceId == p.Id && m.UserId == userId))
            .OrderBy(p => p.Id)
            .ToList()
            .Select(ToDto)
            .ToList();
    }

    private OwnedPlaceDto ToDto(Place place) => new(
        place.Id,
        place.Name,
        place.Description,
        place.Address,
        place.Latitude,
        place.Longtitude,
        place.Hidden,
        place.AcceptingOrders,
        _schedule.IsOpen(place),
        PlaceDtoMapper.MapHours(place));
}

