using Microsoft.EntityFrameworkCore;

using FoodOrderingApi.Data;

using FoodOrderingApi.Dtos.Places;
using FoodOrderingApi.Mappers;

namespace FoodOrderingApi.Services.Places;

public class PlaceService
{
    private readonly AppDbContext _db;
    private readonly PlaceScheduleService _schedule;
    public PlaceService(AppDbContext db, PlaceScheduleService schedule)
    {
        _db = db;
        _schedule = schedule;
    }

    // Hidden places are invisible to customers.
    public List<PlaceSummaryDto> GetAll()
    {
        return _db.Places
            .Where(p => !p.Hidden)
            .Include(p => p.OpeningHours)
            .OrderBy(p => p.Id)
            .ToList()
            .Select(p => PlaceDtoMapper.MapToSummaryDto(p, _schedule.IsOpen(p)))
            .ToList();
    }

    public PublicPlaceDto? Get(int id)
    {   
        // Filter the hidden as well;
        var place = _db.Places
            .Include(p => p.OpeningHours)
            .Include(p => p.ProductCategories.Where(pc => !pc.Hidden))
            .ThenInclude(pc => pc.Products.Where(pr => !pr.Hidden))
            .FirstOrDefault(p => p.Id == id && !p.Hidden);

        if (place == null)
            return null;

        return PlaceDtoMapper.MapToPublicDto(place, _schedule.IsOpen(place));
    }
    
}