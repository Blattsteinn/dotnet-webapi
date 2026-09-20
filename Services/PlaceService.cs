using Microsoft.EntityFrameworkCore;

using FoodOrderingApi.Data;
using FoodOrderingApi.Models;
using FoodOrderingApi.DTOs;

using FoodOrderingApi.Dtos.Places;

namespace FoodOrderingApi.Services;

public class PlaceService
{
    private readonly AppDbContext _db;
    public PlaceService(AppDbContext db)
    {
        _db = db;
    }
    
    public List<Place> GetAll()
    {
        return _db.Places.ToList();
    }

    public PlaceResponse? Get(int id)
    {
        var place = _db.Places
        .Include(p => p.ProductCategories
             .Where(pc => !pc.Hidden))
        .ThenInclude(pc => pc.Products
            .Where(p => !p.Hidden))
        .FirstOrDefault(p => p.Id == id);

        if (place == null)
            return null;

        return new PlaceResponse(
            place.Id,
            place.Latitude,
            place.Longtitude,
            place.Name,
            place.Description,
            place.ProductCategories
                .Select(pc => new ProductCategoryResponse(
                    pc.Id,
                    pc.Name,
                    pc.Description,
                    pc.Hidden,
                    pc.Products
                        .Select(p => new ProductResponse(
                            p.Id,
                            p.Name,
                            p.Description,
                            p.Hidden,
                            p.Price
                        ))
                        .ToList()
                ))
                .ToList()
        );
    }
    public Place Add(Place place)
    {
        _db.Places.Add(place);
        _db.SaveChanges();

        return place;
    }
    public Place? Update(int id, UpdatePlaceDto placeDto)
    {
        Place? place = _db.Places.Find(id);
        if (place == null)
            return null;
        
        if (placeDto.Latitude.HasValue)
            place.Latitude = placeDto.Latitude.Value;

        if (placeDto.Longtitude.HasValue)
            place.Longtitude = placeDto.Longtitude.Value;

        if (placeDto.Name != null)
            place.Name = placeDto.Name;

        if (placeDto.Description != null)
            place.Description = placeDto.Description;

        _db.SaveChanges();

        return place;
    }
    
    public bool Delete(int id)
    {
        Place? place = _db.Places.Find(id);
        if (place == null)
            return false;
        
        _db.Places.Remove(place);
        _db.SaveChanges();
        return true;

    }
}