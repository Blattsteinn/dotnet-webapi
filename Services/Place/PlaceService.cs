using Microsoft.EntityFrameworkCore;

using FoodOrderingApi.Data;
using FoodOrderingApi.Models.Places;

using FoodOrderingApi.Dtos.Places;
using FoodOrderingApi.Mappers;

namespace FoodOrderingApi.Services.Places;

public class PlaceService
{
    private readonly AppDbContext _db;
    public PlaceService(AppDbContext db)
    {
        _db = db;
    }

    // Hidden places are invisible to customers.
    public List<Place> GetAll()
    {
        return _db.Places.Where(p => !p.Hidden).ToList();
    }

    public GetPlaceDto? Get(int id)
    {   
        // Filter the hidden as well;
        var place = _db.Places
            .Include(p => p.ProductCategories.Where(pc => !pc.Hidden))
            .ThenInclude(pc => pc.Products.Where(pr => !pr.Hidden))
            .FirstOrDefault(p => p.Id == id && !p.Hidden);

        if (place == null)
            return null;

        return PlaceDtoMapper.MapToGetDto(place);
    }
    
}