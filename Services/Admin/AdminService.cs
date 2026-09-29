using FoodOrderingApi.Data;

using FoodOrderingApi.Models.Orders;
using FoodOrderingApi.Models.Places;

using FoodOrderingApi.Dtos.Orders;
using FoodOrderingApi.Dtos.Places;
using FoodOrderingApi.Dtos.Admin;

using FoodOrderingApi.Mappers;
using Microsoft.EntityFrameworkCore;


namespace FoodOrderingApi.Services.Admin;
public enum AdminUpdatePlaceResult
{
    Success,
    PlaceNotFound,
    UnknownChildId
}

public class AdminService
{
    private readonly AppDbContext _db;
    public AdminService(AppDbContext db)
    {
        _db = db;
    }

    // Not nested. Admin sees all places, hidden or not.
    public List<Place> GetAllPlaces(int ownerId)
    {
        List<Place> places = _db.Places.ToList();
        return places;
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

    public Place AddPlace(Place place)
    {
        _db.Places.Add(place);
        _db.SaveChanges();

        return place; 
    }
    public (AdminUpdatePlaceResult, GetPlaceDto?) Update(int id, AdminUpdatePlaceDto dto)
    {
        Place? place = _db.Places
            .Include(p => p.ProductCategories)
                .ThenInclude(pc => pc.Products)
                    .ThenInclude(p => p.AddOnCategories)
                        .ThenInclude(ac => ac.AddOns)
            .FirstOrDefault(p => p.Id == id);

        if (place == null)
            return (AdminUpdatePlaceResult.PlaceNotFound, null);

        // Validate before mutating anything.
        if (!AllChildIdsValid(place, dto))
            return (AdminUpdatePlaceResult.UnknownChildId, null);

        place.Name        = dto.Name;
        place.Latitude    = dto.Latitude;
        place.Longtitude  = dto.Longtitude;
        place.Description = dto.Description;

        SyncCategories(place, dto.ProductCategories);
        _db.SaveChanges();

        var reloaded = _db.Places
            .Include(p => p.ProductCategories)
                .ThenInclude(pc => pc.Products)
                    .ThenInclude(p => p.AddOnCategories)
                        .ThenInclude(ac => ac.AddOns)
            .First(p => p.Id == id);

        return (AdminUpdatePlaceResult.Success, PlaceDtoMapper.MapToGetDto(reloaded));
    }

    // Returns false if the place does not exist.
    public bool Delete(int id)
    {
        throw new NotImplementedException();
    }




    // SUTVARKYTI (KITAM FAILE IG?)
    private static bool AllChildIdsValid(Place place, AdminUpdatePlaceDto dto)
    {
        var catsById = place.ProductCategories.ToDictionary(c => c.Id);

        foreach (var catDto in dto.ProductCategories)
        {
            if (catDto.Id == 0) continue;                    // new node, nothing to validate
            if (!catsById.TryGetValue(catDto.Id, out var cat)) return false;

            var prodsById = cat.Products.ToDictionary(p => p.Id);
            foreach (var prodDto in catDto.Products)
            {
                if (prodDto.Id == 0) continue;
                if (!prodsById.TryGetValue(prodDto.Id, out var prod)) return false;

                var aocsById = prod.AddOnCategories.ToDictionary(a => a.Id);
                foreach (var aocDto in prodDto.AddOnCategories)
                {
                    if (aocDto.Id == 0) continue;
                    if (!aocsById.TryGetValue(aocDto.Id, out var aoc)) return false;

                    var addOnsById = aoc.AddOns.ToDictionary(a => a.Id);
                    foreach (var addOnDto in aocDto.AddOns)
                    {
                        if (addOnDto.Id == 0) continue;
                        if (!addOnsById.ContainsKey(addOnDto.Id)) return false;
                    }
                }
            }
        }
        return true;
    }

    private void SyncCategories(Place place, List<AdminUpdateCategoryDto> incoming)
    {
        var deleteIds = incoming.Where(c => c.Destroy && c.Id != 0).Select(c => c.Id).ToHashSet();
        _db.ProductCategories.RemoveRange(place.ProductCategories.Where(c => deleteIds.Contains(c.Id)));

        foreach (var catDto in incoming.Where(c => !c.Destroy))
        {
            ProductCategory category;
            if (catDto.Id == 0)
            {
                category = new ProductCategory { Name = catDto.Name };
                place.ProductCategories.Add(category);
            }
            else
            {
                // Existence validated up front, so First is safe here.
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
        var deleteIds = incoming.Where(p => p.Destroy && p.Id != 0).Select(p => p.Id).ToHashSet();
        _db.Products.RemoveRange(category.Products.Where(p => deleteIds.Contains(p.Id)));

        foreach (var prodDto in incoming.Where(p => !p.Destroy))
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
        var deleteIds = incoming.Where(a => a.Destroy && a.Id != 0).Select(a => a.Id).ToHashSet();
        _db.AddOnCategories.RemoveRange(product.AddOnCategories.Where(a => deleteIds.Contains(a.Id)));

        foreach (var aocDto in incoming.Where(a => !a.Destroy))
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
        var deleteIds = incoming.Where(a => a.Destroy && a.Id != 0).Select(a => a.Id).ToHashSet();
        _db.AddOns.RemoveRange(aoc.AddOns.Where(a => deleteIds.Contains(a.Id)));

        foreach (var addOnDto in incoming.Where(a => !a.Destroy))
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