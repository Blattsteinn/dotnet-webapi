
using FoodOrderingApi.Data;
using FoodOrderingApi.Models;
using FoodOrderingApi.Dtos.AddOn;

namespace FoodOrderingApi.Services;
public class AddOnService
{
    private readonly AppDbContext _db;
    public AddOnService(AppDbContext db)
    {
        _db = db;
    }

    public List<AddOn>? GetAll(int addOnCategoryId)
    {   
        bool addOnCategoryExists = _db.AddOnCategories.Any(a => a.Id == addOnCategoryId);
        if(!addOnCategoryExists)
            return null;

        List<AddOn> addOns = _db.AddOns
            .Where(a => a.AddOnCategoryId == addOnCategoryId)
            .ToList();
        
        return addOns;
    }

    public AddOn? Get(int id, int addOnCategoryId)
    {
        AddOn? addOn = _db.AddOns.Find(id);
        if(addOn == null || addOn.AddOnCategoryId != addOnCategoryId)
            return null;

        return addOn;
        
    }

    public AddOn? Add(AddOn addOn, int addOnCategoryId)
    {   
        AddOnCategory? addOnCategory = _db.AddOnCategories.Find(addOnCategoryId);

        if (addOnCategory == null)
            return null;

        addOn.AddOnCategoryId = addOnCategoryId;
        _db.AddOns.Add(addOn);
        _db.SaveChanges();
        return addOn;
    }

    public AddOn? Update(int id, int addOnCategoryId, UpdateAddOn updateAddOnDto)
    {
        AddOn? updatedAddOn = Get(id, addOnCategoryId);
        if (updatedAddOn == null)
            return null;

        if (updateAddOnDto.Name != null)
            updatedAddOn.Name = updateAddOnDto.Name;

        if (updateAddOnDto.Price.HasValue)
            updatedAddOn.Price = updateAddOnDto.Price.Value;

        if (updateAddOnDto.Hidden.HasValue)
            updatedAddOn.Hidden = updateAddOnDto.Hidden.Value;

        _db.SaveChanges();

        return updatedAddOn;
    }

    public bool Delete(int id, int addOnCategoryId)
    {   
        AddOn? addOn = Get(id, addOnCategoryId);
        if(addOn == null)
            return false;

        _db.AddOns.Remove(addOn);
        _db.SaveChanges();

        return true;
    }
}