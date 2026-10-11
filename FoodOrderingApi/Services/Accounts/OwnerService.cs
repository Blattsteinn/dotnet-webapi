using FoodOrderingApi.Data;
using FoodOrderingApi.Dtos.Accounts.Owner;
using FoodOrderingApi.Dtos.Places;
using FoodOrderingApi.Mappers;
using FoodOrderingApi.Models.Places;
using FoodOrderingApi.Models.Users;
using FoodOrderingApi.Services.Places;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace FoodOrderingApi.Services.Accounts;

public enum OwnerPlaceResult
{
    Success,
    PlaceNotFound,
    NotTheOwner,
    UnknownChildId
}

public enum ManagerResult
{
    Success,
    PlaceNotFound,
    NotTheOwner,
    UserNotFound
}

public enum OwnerHoursResult
{
    Success,
    PlaceNotFound,
    InvalidSchedule
}

// Every operation here is scoped to one owner id, which the controller always
// takes from the caller's token — never from the request body.
public class OwnerService
{
    private readonly AppDbContext _db;
    private readonly PlaceScheduleService _schedule;
    private readonly UserManager<ApplicationUser> _userManager;

    public OwnerService(AppDbContext db, PlaceScheduleService schedule, UserManager<ApplicationUser> userManager)
    {
        _db = db;
        _schedule = schedule;
        _userManager = userManager;
    }

    // Owners rows are the source of truth
    public List<OwnedPlaceDto> GetOwnedPlaces(string ownerId)
    {
        return _db.Places
            .Include(p => p.OpeningHours)
            .Where(p => p.Ownership != null && p.Ownership.OwnerId == ownerId)
            .OrderBy(p => p.Id)
            .ToList()
            .Select(ToDto)
            .ToList();
    }

    public PrivatePlaceDto? GetPlace(string ownerId, int placeId)
    {
        Place? place = _db.Places
            .Include(p => p.Ownership)
            .Include(p => p.OpeningHours)
            .Include(p => p.ProductCategories)
                .ThenInclude(pc => pc.Products)
            .FirstOrDefault(p => p.Id == placeId);

        if (place == null || place.Ownership?.OwnerId != ownerId)
            return null;

        return PlaceDtoMapper.MapToPrivateDto(place, _schedule.IsOpen(place));
    }

    public OwnedPlaceDto CreatePlace(string ownerId, CreatePlaceDto dto)
    {
        var place = new Place
        {
            Name        = dto.Name,
            Description = dto.Description,
            Address     = dto.Address,
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
    public (OwnerPlaceResult, PrivatePlaceDto?) UpdatePlace(string ownerId, int placeId, OwnerUpdatePlaceDto dto)
    {
        Place? place = _db.Places
            .Include(p => p.Ownership)
            .Include(p => p.OpeningHours)
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
            .Include(p => p.OpeningHours)
            .Include(p => p.ProductCategories)
                .ThenInclude(pc => pc.Products)
            .First(p => p.Id == placeId);

        return (OwnerPlaceResult.Success, PlaceDtoMapper.MapToPrivateDto(reloaded, _schedule.IsOpen(reloaded)));
    }

    // ---------------------------------------------------------------------------
    // ---------------------------------------------------------------------------
    // WEEKLY OPENING HOURS
    // ---------------------------------------------------------------------------

    // Fully replaces the place's weekly schedule (delete + insert, one
    // transaction). An empty list clears it, which leaves the place closed.
    public (OwnerHoursResult, List<OpeningHourDto>?, string?) ReplaceHours(
        string ownerId, int placeId, SetOpeningHoursDto dto)
    {
        Place? place = _db.Places
            .Include(p => p.Ownership)
            .Include(p => p.OpeningHours)
            .FirstOrDefault(p => p.Id == placeId);

        // A place that is not this owner's is reported exactly like a missing
        // one, so place ids cannot be enumerated.
        if (place == null || place.Ownership?.OwnerId != ownerId)
            return (OwnerHoursResult.PlaceNotFound, null, null);

        // Validate the whole payload before touching the database.
        string? error = PlaceScheduleService.ValidateSchedule(dto.Hours ?? []);
        if (error != null)
            return (OwnerHoursResult.InvalidSchedule, null, error);

        using var transaction = _db.Database.BeginTransaction();

        _db.PlaceOpeningHours.RemoveRange(place.OpeningHours);
        foreach (OpeningHourDto hour in dto.Hours ?? [])
        {
            _db.PlaceOpeningHours.Add(new PlaceOpeningHour
            {
                PlaceId = placeId,
                Day = hour.Day,
                OpensAt = hour.OpensAt,
                ClosesAt = hour.ClosesAt
            });
        }

        _db.SaveChanges();
        transaction.Commit();

        List<OpeningHourDto> saved = (dto.Hours ?? [])
            .OrderBy(hour => hour.Day)
            .ThenBy(hour => hour.OpensAt)
            .ToList();

        return (OwnerHoursResult.Success, saved, null);
    }

    // ---------------------------------------------------------------------------
    // ---------------------------------------------------------------------------
    // ADDING / REMOVING / LISTING USERS AS MANAGERS
    // ---------------------------------------------------------------------------

    public List<ManagerDto>? GetManagers(string ownerId, int placeId)
    {
        Place? place = _db.Places
            .Include(p => p.Ownership)
            .FirstOrDefault(p => p.Id == placeId);

        if (place == null || place.Ownership?.OwnerId != ownerId)
            return null;

        return _db.Managers
            .Where(m => m.PlaceId == placeId)
            .OrderBy(m => m.UserId)
            .Select(m => new ManagerDto(m.UserId, m.User.UserName, m.User.Email))
            .ToList();
    }

    // Assignment is by email. The user id is resolved here, never taken from the
    // request, so the client never needs to know internal ids.
    public async Task<ManagerResult> AssignManagerAsync(string ownerId, int placeId, string email)
    {
        Place? place = _db.Places
            .Include(p => p.Ownership)
            .FirstOrDefault(p => p.Id == placeId);

        if (place == null)
            return ManagerResult.PlaceNotFound;

        if (place.Ownership?.OwnerId != ownerId)
            return ManagerResult.NotTheOwner;

        ApplicationUser? user = await _userManager.FindByEmailAsync(email);
        if (user == null)
            return ManagerResult.UserNotFound;

        bool alreadyManager = _db.Managers
            .Any(m => m.PlaceId == placeId && m.UserId == user.Id);

        if (!alreadyManager)
        {
            _db.Managers.Add(new PlaceManager { PlaceId = placeId, UserId = user.Id });
            _db.SaveChanges();
        }

        return ManagerResult.Success;
    }

    public ManagerResult RemoveManager(string ownerId, int placeId, string userId)
    {
        Place? place = _db.Places
            .Include(p => p.Ownership)
            .FirstOrDefault(p => p.Id == placeId);

        if (place == null)
            return ManagerResult.PlaceNotFound;

        if (place.Ownership?.OwnerId != ownerId)
            return ManagerResult.NotTheOwner;

        if (!_db.Users.Any(u => u.Id == userId))
            return ManagerResult.UserNotFound;

        PlaceManager? manager = _db.Managers
            .FirstOrDefault(m => m.PlaceId == placeId && m.UserId == userId);

        if (manager != null)
        {
            _db.Managers.Remove(manager);
            _db.SaveChanges();
        }

        return ManagerResult.Success;
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

