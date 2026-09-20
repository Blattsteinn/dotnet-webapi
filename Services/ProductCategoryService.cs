using FoodOrderingApi.Data;
using FoodOrderingApi.Models;
using FoodOrderingApi.Dtos.ProductCategory;

namespace FoodOrderingApi.Services;

public class ProductCategoryService
{
    private readonly AppDbContext _db;
    public ProductCategoryService(AppDbContext db)
    {
        _db = db;
    }

    public List<ProductCategory>? GetAll(int placeId)
    {   
        bool placeExists = _db.Places.Any(p => p.Id == placeId);
        if(!placeExists)
            return null;

        return _db.ProductCategories
            .Where(pc => pc.PlaceId == placeId)
            .ToList();
    }

    public ProductCategory? Get(int id, int placeId)
    {   
        ProductCategory? productCategory = _db.ProductCategories.Find(id);
        
        if(productCategory == null || productCategory.PlaceId != placeId)
            return null;

        return productCategory;
    }

    public ProductCategory? Add(ProductCategory productCategory, int placeId)
    {
        Place? place = _db.Places.Find(placeId);

        if (place == null)
            return null;

        productCategory.PlaceId = placeId;
        _db.ProductCategories.Add(productCategory);
        _db.SaveChanges();

        return productCategory;
    }

    public ProductCategory? Update(int id, int placeId, UpdateProductCategory productCategoryDto)
    {
        ProductCategory? productCategory = Get(id, placeId);
        if(productCategory == null)
            return null;

        if (productCategoryDto.Name != null)
            productCategory.Name = productCategoryDto.Name;

        if (productCategoryDto.Description != null)
            productCategory.Description = productCategoryDto.Description;

        if (productCategoryDto.Hidden.HasValue)
            productCategory.Hidden = productCategoryDto.Hidden.Value;

        _db.SaveChanges();

        return productCategory;
    }

    public bool Delete(int id, int placeId)
    {
        ProductCategory? productCategory = Get(id, placeId);
        if(productCategory == null)
            return false;

        _db.ProductCategories.Remove(productCategory);
        _db.SaveChanges();

        return true;
    }   

}