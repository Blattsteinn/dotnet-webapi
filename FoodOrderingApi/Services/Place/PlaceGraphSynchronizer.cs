using FoodOrderingApi.Data;
using FoodOrderingApi.Dtos.Accounts.Owner;
using FoodOrderingApi.Models.Places;

namespace FoodOrderingApi.Services.Places;

// The nested ProductCategory -> Product -> AddOnCategory -> AddOn graph is
// written the same way no matter who is allowed to touch it. Keeping it here
// means the admin path and the owner path can never drift apart: they are the
// same code, only the permission check in front of it differs.
public static class PlaceGraphSynchronizer
{
    // Validates before anything is mutated, so a bad payload cannot leave a
    // half-applied update behind.
    public static bool AllChildIdsValid(Place place, OwnerUpdatePlaceDto dto)
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

    // Applies the whole update payload to the place: the place-level fields
    // first (Hidden included — an owner may hide their own place), then the
    // nested graph. Keeping the scalars here too is what stops "what an update
    // means" from being split across two files.
    public static void Sync(AppDbContext db, Place place, OwnerUpdatePlaceDto dto)
    {
        place.Name        = dto.Name;
        place.Latitude    = dto.Latitude;
        place.Longtitude  = dto.Longtitude;
        place.Description = dto.Description;
        place.Address     = dto.Address;
        place.Hidden      = dto.Hidden;

        // Unlike Hidden, an omitted acceptingOrders must NOT reset the stored
        // value — it is a separate switch the owner may not be touching.
        if (dto.AcceptingOrders is bool acceptingOrders)
            place.AcceptingOrders = acceptingOrders;

        SyncCategories(db, place, dto.ProductCategories);
    }

    private static void SyncCategories(AppDbContext db, Place place, List<OwnerUpdateCategoryDto> incoming)
    {
        var deleteIds = incoming.Where(c => c.Destroy && c.Id != 0).Select(c => c.Id).ToHashSet();
        db.ProductCategories.RemoveRange(place.ProductCategories.Where(c => deleteIds.Contains(c.Id)));

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

            SyncProducts(db, category, catDto.Products);
        }
    }

    private static void SyncProducts(AppDbContext db, ProductCategory category, List<OwnerUpdateProductDto> incoming)
    {
        var deleteIds = incoming.Where(p => p.Destroy && p.Id != 0).Select(p => p.Id).ToHashSet();
        db.Products.RemoveRange(category.Products.Where(p => deleteIds.Contains(p.Id)));

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

            SyncAddOnCategories(db, product, prodDto.AddOnCategories);
        }
    }

    private static void SyncAddOnCategories(AppDbContext db, Product product, List<OwnerUpdateAddOnCategoryDto> incoming)
    {
        var deleteIds = incoming.Where(a => a.Destroy && a.Id != 0).Select(a => a.Id).ToHashSet();
        db.AddOnCategories.RemoveRange(product.AddOnCategories.Where(a => deleteIds.Contains(a.Id)));

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

            SyncAddOns(db, aoc, aocDto.AddOns);
        }
    }

    private static void SyncAddOns(AppDbContext db, AddOnCategory aoc, List<OwnerUpdateAddOnDto> incoming)
    {
        var deleteIds = incoming.Where(a => a.Destroy && a.Id != 0).Select(a => a.Id).ToHashSet();
        db.AddOns.RemoveRange(aoc.AddOns.Where(a => deleteIds.Contains(a.Id)));

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
