
using FoodOrderingApi.Data;
using FoodOrderingApi.Models;
using FoodOrderingApi.Dtos.AddOnCategory;

namespace FoodOrderingApi.Services;

public class AddOnCategoryService
{
    private readonly AppDbContext _db;
    public AddOnCategoryService(AppDbContext db)
    {
        _db = db;
    }

    public List<AddOnCategory>? GetAll(int productId)
    {   
        bool productExists = _db.Products.Any(p => p.Id == productId);
        if(!productExists)
            return null;

        List<AddOnCategory> addOnCategories = _db.AddOnCategories
        .Where(a => a.ProductId == productId)
        .ToList();

        return addOnCategories;
    }

    public AddOnCategory? Get(int id, int productId)
    {
        AddOnCategory? addOnCategory = _db.AddOnCategories.Find(id);

        if(addOnCategory == null || addOnCategory.ProductId != productId)
            return null;
        
        return addOnCategory;
    }

    public AddOnCategory? Add(int productId, AddOnCategory addOnCategory)
    {
        Product? product = _db.Products.Find(productId);
        if(product == null)
            return null;
        
        addOnCategory.ProductId = productId;
        _db.AddOnCategories.Add(addOnCategory);
        _db.SaveChanges();

        return addOnCategory;
    }

    public AddOnCategory? Update(int id, int productId, UpdateAddOnCategory updateAddOnCategoryDto)
    {
        AddOnCategory? updatedAddOnCategory = Get(id, productId);
        if(updatedAddOnCategory == null)
            return null;

        if (updateAddOnCategoryDto.Name != null)
            updatedAddOnCategory.Name = updateAddOnCategoryDto.Name;

        if (updateAddOnCategoryDto.Required.HasValue)
            updatedAddOnCategory.Required = updateAddOnCategoryDto.Required.Value;

        if (updateAddOnCategoryDto.Multichoice.HasValue)
            updatedAddOnCategory.Multichoice = updateAddOnCategoryDto.Multichoice.Value;

        _db.SaveChanges();

        return updatedAddOnCategory;
    }

    public bool Delete(int id, int productId)
    {
        AddOnCategory? addOnCategory = Get(id, productId);
        if(addOnCategory == null)
            return false;

        _db.AddOnCategories.Remove(addOnCategory);
        _db.SaveChanges();
        return true;
    }
}