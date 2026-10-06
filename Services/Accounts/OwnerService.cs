using FoodOrderingApi.Data;
using FoodOrderingApi.Dtos.Accounts.Owner;
using FoodOrderingApi.Dtos.Places;
using FoodOrderingApi.Mappers;
using FoodOrderingApi.Models.Places;
using FoodOrderingApi.Services.Places;
using Microsoft.EntityFrameworkCore;

namespace FoodOrderingApi.Services.Accounts;

public enum OwnerPlaceResult
{
    Success,
    PlaceNotFound,
    NotTheOwner,
    UnknownChildId
}

public enum ManagerRoleResult
{
    Success,
    PlaceNotFound,
    NotTheOwner,
    UserNotFound
}
// Every operation here is scoped to one owner id, which the controller always
// takes from the caller's token — never from the request body.
public class OwnerService
{
    private readonly AppDbContext _db;

    public OwnerService(AppDbContext db)
    {
        _db = db;
    }

    // Owners rows are the source of truth
    public List<OwnedPlaceDto> GetOwnedPlaces(string ownerId)
    {
        return _db.Places
            .Where(p => p.Ownership != null && p.Ownership.OwnerId == ownerId)
            .OrderBy(p => p.Id)
            .Select(p => new OwnedPlaceDto(
                p.Id,
                p.Name,
                p.Description,
                p.Latitude,
                p.Longtitude,
                p.Hidden))
            .ToList();
    }

    // Returns null when the place is missing OR when it belongs to someone
    public GetPlaceDto? GetPlace(string ownerId, int placeId)
    {
        Place? place = _db.Places
            .Include(p => p.Ownership)
            .Include(p => p.ProductCategories)
                .ThenInclude(pc => pc.Products)
            .FirstOrDefault(p => p.Id == placeId);

        if (place == null || place.Ownership?.OwnerId != ownerId)
            return null;

        return PlaceDtoMapper.MapToGetDto(place);
    }

    public OwnedPlaceDto CreatePlace(string ownerId, CreatePlaceDto dto)
    {
        var place = new Place
        {
            Name        = dto.Name,
            Description = dto.Description,
            Latitude    = dto.Latitude,
            Longtitude  = dto.Longtitude,
            Ownership   = new PlaceOwnership { OwnerId = ownerId }
        };

        _db.Places.Add(place);
        _db.SaveChanges();

        return ToDto(place);
    }

    // The permission question here is per place — "does this person own THIS
    // row?" — not global. The Owner role only got them through the door.
    public (OwnerPlaceResult, GetPlaceDto?) UpdatePlace(string ownerId, int placeId, OwnerUpdatePlaceDto dto)
    {
        Place? place = _db.Places
            .Include(p => p.Ownership)
            .Include(p => p.ProductCategories)
                .ThenInclude(pc => pc.Products)
                    .ThenInclude(p => p.AddOnCategories)
                        .ThenInclude(ac => ac.AddOns)
            .FirstOrDefault(p => p.Id == placeId);

        if (place == null)
            return (OwnerPlaceResult.PlaceNotFound, null);

        if (place.Ownership?.OwnerId != ownerId)
            return (OwnerPlaceResult.NotTheOwner, null);

        // Long validation to check if everything belongs to one place 
        if (!PlaceGraphSynchronizer.AllChildIdsValid(place, dto))
            return (OwnerPlaceResult.UnknownChildId, null);

        // Applies the place fields, the Hidden flag and the nested graph.
        PlaceGraphSynchronizer.Sync(_db, place, dto);
        _db.SaveChanges();

        var reloaded = _db.Places
            .Include(p => p.ProductCategories)
                .ThenInclude(pc => pc.Products)
            .First(p => p.Id == placeId);

        return (OwnerPlaceResult.Success, PlaceDtoMapper.MapToGetDto(reloaded));
    }

    // Used by the owner to assign role as a manager.
    // Manager is a per-place relationship recorded in the Managers table, so the
    // caller must own the place and the target user must exist.
    public ManagerRoleResult AddManager(string ownerId, int placeId, string userId)
    {
        Place? place = _db.Places
            .Include(p => p.Ownership)
            .FirstOrDefault(p => p.Id == placeId);

        if (place == null)
            return ManagerRoleResult.PlaceNotFound;

        if (place.Ownership?.OwnerId != ownerId)
            return ManagerRoleResult.NotTheOwner;

        if (!_db.Users.Any(u => u.Id == userId))
            return ManagerRoleResult.UserNotFound;

        // Idempotent: promoting an existing manager is a no-op.
        bool alreadyManager = _db.Managers
            .Any(m => m.PlaceId == placeId && m.UserId == userId);

        if (!alreadyManager)
        {
            _db.Managers.Add(new PlaceManager { PlaceId = placeId, UserId = userId });
            _db.SaveChanges();
        }

        return ManagerRoleResult.Success;
    }
    private static OwnedPlaceDto ToDto(Place place) => new(
        place.Id,
        place.Name,
        place.Description,
        place.Latitude,
        place.Longtitude,
        place.Hidden);
}
