using FoodOrderingApi.Data;

using FoodOrderingApi.Models.Orders;
using FoodOrderingApi.Models.Places;

using FoodOrderingApi.Dtos.Orders;
using FoodOrderingApi.Dtos.Places;
using FoodOrderingApi.Dtos.Admin;

using FoodOrderingApi.Mappers;
using Microsoft.EntityFrameworkCore;


namespace FoodOrderingApi.Services.Admin;

public class AdminService
{
    private readonly AppDbContext _db;
    public AdminService(AppDbContext db)
    {
        _db = db;
    }

    // Not nested. Admin sees all places, hidden or not.
    public List<Place>? GetAll()
    {
        throw new NotImplementedException();
    }

    public Place? GetPlace(int id)
    {   
        Place? place = _db.Places
            .Include(p => p.ProductCategories)
            .ThenInclude(pc => pc.Products)
            .ThenInclude(p => p.AddOnCategories)
            .ThenInclude(ac => ac.AddOns)
            .FirstOrDefault(p => p.Id == id);

            return place;
    }

    // PATCH semantics: only non-null fields are applied.
    public GetPlaceDto? Update(int id, AdminUpdatePlaceDto dto)
    {
        // Load the whole tree WITH tracking (default). We'll mutate it in place.
        Place? place = _db.Places
            .Include(p => p.ProductCategories)
                .ThenInclude(pc => pc.Products)
                    .ThenInclude(p => p.AddOnCategories)
                        .ThenInclude(ac => ac.AddOns)
            .FirstOrDefault(p => p.Id == id);

        if (place == null) return null;

        // 1. Flat fields
        place.Name        = dto.Name;
        place.Latitude    = dto.Latitude;
        place.Longtitude  = dto.Longtitude;
        place.Description = dto.Description;

        // 2. Categories
        SyncCategories(place, dto.ProductCategories);

        _db.SaveChanges();

        // Re-load with includes (or reuse `place`) and map. Simplest: reload.
        var reloaded = _db.Places
            .Include(p => p.ProductCategories)
                .ThenInclude(pc => pc.Products)
                    .ThenInclude(p => p.AddOnCategories)
                        .ThenInclude(ac => ac.AddOns)
            .First(p => p.Id == id);

        return PlaceDtoMapper.MapToGetDto(reloaded);
    }

    // Returns false if the place does not exist.
    public bool Delete(int id)
    {
        throw new NotImplementedException();
    }


    // Helpers for renewing
    private void SyncCategories(Place place, List<AdminUpdateCategoryDto> incoming)
    {
        // Delete categories that weren't sent
        var incomingIds = incoming.Where(c => c.Id != 0).Select(c => c.Id).ToHashSet();
        var toDelete = place.ProductCategories.Where(c => !incomingIds.Contains(c.Id)).ToList();
        _db.ProductCategories.RemoveRange(toDelete);

        foreach (var catDto in incoming)
        {
            ProductCategory category;
            if (catDto.Id == 0)
            {
                category = new ProductCategory { Name = catDto.Name };
                place.ProductCategories.Add(category);
            }
            else
            {
                category = place.ProductCategories.First(c => c.Id == catDto.Id);
            }

            category.Name        = catDto.Name;
            category.Description = catDto.Description;
            category.Hidden      = catDto.Hidden;

            SyncProducts(category, catDto.Products);
        }
    }

    private void SyncProducts(ProductCategory category, List<AdminUpdateProductDto> incoming)
    {
        var incomingIds = incoming.Where(p => p.Id != 0).Select(p => p.Id).ToHashSet();
        var toDelete = category.Products.Where(p => !incomingIds.Contains(p.Id)).ToList();
        _db.Products.RemoveRange(toDelete);

        foreach (var prodDto in incoming)
        {
            Product product;
            if (prodDto.Id == 0)
            {
                product = new Product { Name = prodDto.Name };
                category.Products.Add(product);
            }
            else
            {
                product = category.Products.First(p => p.Id == prodDto.Id);
            }

            product.Name        = prodDto.Name;
            product.Description = prodDto.Description;
            product.Hidden      = prodDto.Hidden;
            product.Price       = prodDto.Price;

            SyncAddOnCategories(product, prodDto.AddOnCategories);
        }
    }

    private void SyncAddOnCategories(Product product, List<AdminUpdateAddOnCategoryDto> incoming)
    {
        var incomingIds = incoming.Where(a => a.Id != 0).Select(a => a.Id).ToHashSet();
        var toDelete = product.AddOnCategories.Where(a => !incomingIds.Contains(a.Id)).ToList();
        _db.AddOnCategories.RemoveRange(toDelete);

        foreach (var aocDto in incoming)
        {
            AddOnCategory aoc;
            if (aocDto.Id == 0)
            {
                aoc = new AddOnCategory { Name = aocDto.Name };
                product.AddOnCategories.Add(aoc);
            }
            else
            {
                aoc = product.AddOnCategories.First(a => a.Id == aocDto.Id);
            }

            aoc.Name        = aocDto.Name;
            aoc.Required    = aocDto.Required;
            aoc.Multichoice = aocDto.Multichoice;

            SyncAddOns(aoc, aocDto.AddOns);
        }
    }

    private void SyncAddOns(AddOnCategory aoc, List<AdminUpdateAddOnDto> incoming)
    {
        var incomingIds = incoming.Where(a => a.Id != 0).Select(a => a.Id).ToHashSet();
        var toDelete = aoc.AddOns.Where(a => !incomingIds.Contains(a.Id)).ToList();
        _db.AddOns.RemoveRange(toDelete);

        foreach (var addOnDto in incoming)
        {
            AddOn addOn;
            if (addOnDto.Id == 0)
            {
                addOn = new AddOn { Name = addOnDto.Name };
                aoc.AddOns.Add(addOn);
            }
            else
            {
                addOn = aoc.AddOns.First(a => a.Id == addOnDto.Id);
            }

            addOn.Name   = addOnDto.Name;
            addOn.Price  = addOnDto.Price;
            addOn.Hidden = addOnDto.Hidden;
        }
    }
}